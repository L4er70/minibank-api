import { useState, useEffect } from 'react';
import api from './api/axios';
import AccountsPanel from './components/AccountsPanel';
import CustomerTable from './components/CustomerTable';
import Header from './components/Header';
import RegisterForm from './components/RegisterForm';
import SearchBar from './components/SearchBar';
import SuccessToast from './components/SuccessToast';

const homeCurrency = 'ALL';

const exchangeRatesToHomeCurrency = {
  ALL: 1,
  EUR: 103.5,
  USD: 95.2,
  GBP: 111.01
};

function App() {
  const [customers, setCustomers] = useState([]);
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [accounts, setAccounts] = useState([]);
  const [totalWealthSummary, setTotalWealthSummary] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [amounts, setAmounts] = useState({});
  const [toasts, setToasts] = useState([]);

  const formatMoney = (value, currency) => {
    try {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency,
        maximumFractionDigits: 2
      }).format(value);
    } catch {
      return `${Number(value).toFixed(2)} ${currency}`;
    }
  };

  const getLocalTotalWealthFallback = (accountList) =>
    accountList.reduce((total, account) => {
      if (!account.isActive) {
        return total;
      }

      const rate = exchangeRatesToHomeCurrency[account.currency] ?? 1;
      return total + account.balance * rate;
    }, 0);

  const parseTotalWealthResponse = (payload, fallbackAmount) => {
    const dto = payload?.data ?? payload ?? {};
    const amount =
      dto.totalWealth ??
      dto.totalBalance ??
      dto.balance ??
      dto.amount ??
      dto.value ??
      fallbackAmount;
    const currency =
      dto.homeCurrency ??
      dto.currency ??
      dto.baseCurrency ??
      homeCurrency;

    return {
      amount: Number(amount ?? fallbackAmount),
      currency: currency || homeCurrency,
      source: payload?.data || payload ? 'backend' : 'fallback'
    };
  };

  const fetchTotalWealthForCustomer = async (customer, accountList = []) => {
    if (!customer) {
      setTotalWealthSummary(null);
      return;
    }

    const fallbackAmount = getLocalTotalWealthFallback(accountList);
    const candidateEndpoints = [
      `/Account/customer/${customer.id}/total-wealth`,
      `/Account/GetTotalWealth?customerId=${customer.id}`,
      `/Account/total-wealth/${customer.id}`,
      `/Account/customer/${customer.id}/total-balance`,
      `/Account/GetTotalBalance?customerId=${customer.id}`
    ];

    for (const endpoint of candidateEndpoints) {
      try {
        const response = await api.get(endpoint);
        setTotalWealthSummary(parseTotalWealthResponse(response.data, fallbackAmount));
        return;
      } catch {
        // Try the next route shape before falling back to the client-side summary.
      }
    }

    setTotalWealthSummary({
      amount: fallbackAmount,
      currency: homeCurrency,
      source: 'fallback'
    });
  };

  const showToast = (message, type = 'info') => {
    const id = `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
    setToasts((currentToasts) => [...currentToasts, { id, message, type }]);
    window.setTimeout(() => {
      setToasts((currentToasts) => currentToasts.filter((toast) => toast.id !== id));
    }, 3500);
  };

  const fetchAccountsForCustomer = async (customer) => {
    if (!customer) return;

    try {
      const response = await api.get(`/Account/customer/${customer.id}`);
      const nextAccounts = response.data.data || [];
      setAccounts(nextAccounts);
      await fetchTotalWealthForCustomer(customer, nextAccounts);
    } catch (error) {
      console.error('Error fetching accounts: ', error);
      setAccounts([]);
      await fetchTotalWealthForCustomer(customer, []);
    }
  };

  const closeAccount = async (accountId) => {
    const response = await api.patch(`/Account/${accountId}/close`);
    return response.data;
  };

  const reopenAccount = async (accountId) => {
    const response = await api.patch(`/Account/${accountId}/reopen`);
    return response.data;
  };

  const transferFunds = async (payload) => {
    const endpoint = payload.toAccounNumber ? '/Account/transfer-external' : '/Account/transfer';
    const response = await api.post(endpoint, payload);
    return response.data;
  };

  // handle transactions
  const handleTransaction = async (accountId, type) => {
    const account = accounts.find((currentAccount) => currentAccount.id === accountId);

    if (account && !account.isActive) {
      showToast('This account is closed. No transactions can be made.', 'error');
      return false;
    }

    const amount = parseFloat(amounts[accountId]);

    if (!amount || amount <= 0) {
      showToast('Please enter a valid amount.', 'error');
      return false;
    }

    try {
      const payload = {
        accountId,
        amount,
        transactionType: type === 'Deposit' ? 0 : 1,
        description: `${type} via Web Portal`
      };

      const response = await api.post('/Account/transactions', payload);

      if (response.data.success) {
        showToast(`${type} successful!`, 'success');
        setAmounts((currentAmounts) => ({ ...currentAmounts, [accountId]: '' }));
        await fetchAccountsForCustomer(selectedCustomer);
        return true;
      }
    } catch (error) {
      showToast(error.response?.data?.message || 'Transaction failed', 'error');
    }

    return false;
  };

  const handleCloseAccount = async (accountId) => {
    try {
      const response = await closeAccount(accountId);

      if (response.success) {
        showToast('Account closed successfully.', 'success');
        setAmounts((currentAmounts) => ({ ...currentAmounts, [accountId]: '' }));
        await fetchAccountsForCustomer(selectedCustomer);
        return true;
      }
    } catch (error) {
      showToast(error.response?.data?.message || 'Could not close account.', 'error');
    }

    return false;
  };

  const handleReopenAccount = async (accountId) => {
    try {
      const response = await reopenAccount(accountId);

      if (response.success) {
        showToast('Account re-opened successfully.', 'success');
        await fetchAccountsForCustomer(selectedCustomer);
        return true;
      }
    } catch (error) {
      showToast(error.response?.data?.message || 'Could not re-open account.', 'error');
    }

    return false;
  };

  const handleTransferFunds = async (payload) => {
    try {
      const response = await transferFunds(payload);

      if (response.success) {
        showToast('Transfer completed successfully.', 'success');
        setAmounts((currentAmounts) => {
          const nextAmounts = {
            ...currentAmounts,
            [payload.fromAccountId]: ''
          };

          if (payload.toAccountId) {
            nextAmounts[payload.toAccountId] = '';
          }

          return nextAmounts;
        });
        await fetchAccountsForCustomer(selectedCustomer);
        return true;
      }
    } catch (error) {
      showToast(error.response?.data?.message || 'Transfer failed.', 'error');
    }

    return false;
  };

  
  
  // 1. Form State
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    personalId: ''
  });

  // Fetch data on load
  const fetchCustomers = async () => {
    try {
      const response = await api.get('/customers');
      setCustomers(response.data.data || []);
    } catch (error) {
      console.error("Error fetching data:", error);
    }
  };

  useEffect(() => {
    fetchCustomers();
  }, []);

  // 2. Handle Input Changes
  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  //filter
  const filteredCustomers = customers.filter((customer) =>
    customer.firstName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    customer.lastName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    customer.personalId.toLowerCase().includes(searchQuery.toLowerCase())
  );

  //handle view account
  const handleViewAccounts = async (customer) => {
    setSelectedCustomer(customer);
    fetchAccountsForCustomer(customer);
  };

  const handleAmountChange = (accountId, value) => {
    setAmounts((currentAmounts) => ({ ...currentAmounts, [accountId]: value }));
  };

  // 3. Submit to Backend
  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await api.post('/customers', formData);
      if (response.data.success) {
        // Clear form and refresh list
        setFormData({ firstName: '', lastName: '', personalId: '' });
        fetchCustomers(); 
        showToast('Customer registered successfully.', 'success');
      }
    } catch (error) {
      showToast('Registration failed. Check if Personal ID already exists.', 'error');
      console.error(error);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 p-8 flex justify-center">
      <SuccessToast toasts={toasts} />
      <div className="w-full max-w-6xl">
        <Header />
        <section className="mb-6 overflow-hidden rounded-[28px] bg-gradient-to-r from-slate-950 via-blue-950 to-slate-900 text-white shadow-xl">
          <div className="grid gap-4 px-6 py-6 md:grid-cols-[1.6fr_1fr] md:px-8">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-blue-200">
                Total Wealth
              </p>
              <div className="mt-3 flex flex-wrap items-end gap-3">
                <h2 className="text-3xl font-black tracking-tight md:text-4xl">
                  {selectedCustomer
                    ? formatMoney(
                        totalWealthSummary?.amount ?? getLocalTotalWealthFallback(accounts),
                        totalWealthSummary?.currency ?? homeCurrency
                      )
                    : formatMoney(0, homeCurrency)}
                </h2>
                <span className="rounded-full border border-white/15 bg-white/10 px-3 py-1 text-xs font-semibold text-blue-100">
                  Home Currency: {totalWealthSummary?.currency ?? homeCurrency}
                </span>
              </div>
              <p className="mt-3 max-w-2xl text-sm text-slate-200">
                {selectedCustomer
                  ? `${selectedCustomer.firstName} ${selectedCustomer.lastName} currently holds ${accounts.filter((account) => account.isActive).length} active account${accounts.filter((account) => account.isActive).length === 1 ? '' : 's'} summarized into one teller-friendly view.`
                  : 'Choose a customer to see their combined balance across active accounts, converted into one clear base currency.'}
              </p>
            </div>
            <div className="grid gap-3 rounded-2xl border border-white/10 bg-white/5 p-4 backdrop-blur-sm">
              <div>
                <p className="text-xs uppercase tracking-[0.22em] text-blue-200">Selected Customer</p>
                <p className="mt-2 text-lg font-semibold text-white">
                  {selectedCustomer
                    ? `${selectedCustomer.firstName} ${selectedCustomer.lastName}`
                    : 'No customer selected'}
                </p>
                {selectedCustomer && (
                  <p className="mt-2 text-xs text-blue-100/80">
                    Source: {totalWealthSummary?.source === 'backend' ? 'Backend total' : 'Local fallback'}
                  </p>
                )}
              </div>
              <div className="grid grid-cols-2 gap-3 text-sm">
                <div className="rounded-2xl bg-white/10 p-3">
                  <p className="text-xs uppercase tracking-[0.18em] text-blue-100">Accounts</p>
                  <p className="mt-2 text-2xl font-bold text-white">{accounts.length}</p>
                </div>
                <div className="rounded-2xl bg-white/10 p-3">
                  <p className="text-xs uppercase tracking-[0.18em] text-blue-100">Active</p>
                  <p className="mt-2 text-2xl font-bold text-white">
                    {accounts.filter((account) => account.isActive).length}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </section>
        <RegisterForm
          formData={formData}
          onChange={handleChange}
          onSubmit={handleSubmit}
        />
        <SearchBar
          value={searchQuery}
          onChange={(event) => setSearchQuery(event.target.value)}
        />
        <CustomerTable
          customers={filteredCustomers}
          onViewAccounts={handleViewAccounts}
        />
        <AccountsPanel
          selectedCustomer={selectedCustomer}
          accounts={accounts}
          amounts={amounts}
          onAmountChange={handleAmountChange}
          onTransaction={handleTransaction}
          onCloseAccount={handleCloseAccount}
          onReopenAccount={handleReopenAccount}
          onTransferFunds={handleTransferFunds}
          onAccountsChanged={fetchAccountsForCustomer}
          onNotify={showToast}
          onClose={() => {
            setSelectedCustomer(null);
            setAccounts([]);
            setTotalWealthSummary(null);
          }}
        />
      </div>
    </div>
  );
}

export default App;
