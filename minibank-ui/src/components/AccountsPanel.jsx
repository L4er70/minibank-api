import { useState } from 'react';
import api from '../api/axios';

/* eslint-disable react/prop-types */
function AccountsPanel({
  selectedCustomer,
  accounts,
  amounts,
  onAmountChange,
  onTransaction,
  onClose
}) {
  const [expandedAccountId, setExpandedAccountId] = useState(null);
  const [historyByAccount, setHistoryByAccount] = useState({});
  const [historyLoadingId, setHistoryLoadingId] = useState(null);

  if (!selectedCustomer) return null;

  const loadHistory = async (accountId) => {
    setHistoryLoadingId(accountId);

    try {
      const response = await api.get(`/Account/${accountId}/transactions`);
      setHistoryByAccount((currentHistory) => ({
        ...currentHistory,
        [accountId]: response.data.data || []
      }));
      setExpandedAccountId(accountId);
    } catch (error) {
      console.error('Could not load transactions', error);
      setHistoryByAccount((currentHistory) => ({
        ...currentHistory,
        [accountId]: []
      }));
      setExpandedAccountId(accountId);
    } finally {
      setHistoryLoadingId(null);
    }
  };

  const toggleHistory = async (accountId) => {
    if (expandedAccountId === accountId) {
      setExpandedAccountId(null);
      return;
    }

    await loadHistory(accountId);
  };

  const handleTransactionClick = async (accountId, type) => {
    const success = await onTransaction(accountId, type);

    if (success && expandedAccountId === accountId) {
      await loadHistory(accountId);
    }
  };

  const formatCurrency = (value) =>
    new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);

  const formatDate = (value) =>
    new Date(value).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit'
    });

  return (
    <div className="mt-12 rounded-lg border-t-4 border-blue-900 bg-white p-6 shadow-inner animate-fadeIn">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-xl font-bold text-gray-800">
          Accounts for {selectedCustomer.firstName} {selectedCustomer.lastName}
        </h2>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
          Close ✕
        </button>
      </div>

      {accounts.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {accounts.map((account) => {
            const transactions = historyByAccount[account.id] || [];
            const isExpanded = expandedAccountId === account.id;
            const isLoadingHistory = historyLoadingId === account.id;

            return (
              <div key={account.id} className="rounded-lg border bg-gray-50 p-4">
                <div className="mb-4 flex items-center justify-between">
                  <div>
                    <p className="text-xs font-bold uppercase text-gray-500">
                      {account.accountType}
                    </p>
                    <p className="text-lg font-mono">{account.accountNumber}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-gray-500">Balance</p>
                    <p className="text-xl font-bold text-green-700">
                      {formatCurrency(account.balance)}
                    </p>
                  </div>
                </div>

                <div className="space-y-3">
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    placeholder="Enter amount"
                    value={amounts[account.id] || ''}
                    onChange={(event) => onAmountChange(account.id, event.target.value)}
                    className="w-full rounded border p-2 outline-none focus:ring-2 focus:ring-blue-500"
                  />
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleTransactionClick(account.id, 'Deposit')}
                      className="flex-1 rounded bg-green-600 px-4 py-2 text-sm font-semibold text-white hover:bg-green-700"
                    >
                      Deposit
                    </button>
                    <button
                      onClick={() => handleTransactionClick(account.id, 'Withdraw')}
                      className="flex-1 rounded bg-red-600 px-4 py-2 text-sm font-semibold text-white hover:bg-red-700"
                    >
                      Withdraw
                    </button>
                  </div>
                  <button
                    onClick={() => toggleHistory(account.id)}
                    className="w-full rounded border border-blue-200 bg-white px-4 py-2 text-sm font-semibold text-blue-700 hover:bg-blue-50"
                  >
                    {isExpanded ? 'Hide History' : 'View History'}
                  </button>
                </div>

                {isExpanded && (
                  <div className="mt-4 rounded-lg border bg-white p-3">
                    <h3 className="mb-3 text-sm font-bold uppercase tracking-wide text-gray-600">
                      Transaction History
                    </h3>

                    {isLoadingHistory ? (
                      <p className="text-sm text-gray-500">Loading transactions...</p>
                    ) : transactions.length > 0 ? (
                      <div className="space-y-2">
                        {transactions.map((transaction) => (
                          <div
                            key={transaction.id}
                            className="flex items-start justify-between rounded border border-gray-100 bg-gray-50 p-3"
                          >
                            <div>
                              <p className="font-semibold text-gray-800">{transaction.type}</p>
                              <p className="text-sm text-gray-500">
                                {transaction.description || 'No description'}
                              </p>
                              <p className="text-xs text-gray-400">
                                {formatDate(transaction.transactionDate)}
                              </p>
                            </div>
                            <p
                              className={`font-semibold ${
                                transaction.type === 'Credit'
                                  ? 'text-green-700'
                                  : 'text-red-700'
                              }`}
                            >
                              {transaction.type === 'Credit' ? '+' : '-'}
                              {formatCurrency(transaction.amount)}
                            </p>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <p className="text-sm text-gray-500">
                        No transactions found for this account.
                      </p>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      ) : (
        <div className="rounded-lg border-2 border-dashed py-8 text-center">
          <p className="mb-4 text-gray-500">No accounts found for this customer.</p>
          <button className="rounded bg-blue-900 px-4 py-2 text-sm text-white">
            + Open New Account
          </button>
        </div>
      )}
    </div>
  );
}

export default AccountsPanel;
