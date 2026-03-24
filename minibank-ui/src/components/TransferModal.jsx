import { useEffect, useRef, useState } from 'react';

const exchangeRates = {
  EUR_ALL: 103.5,
  ALL_EUR: 0.0097,
  USD_ALL: 95.2,
  ALL_USD: 0.105,
  EUR_USD: 1.08,
  USD_EUR: 0.92,
  GBP_ALL: 111.01,
  ALL_GBP: 0.009,
  GBP_EUR: 1.16,
  EUR_GBP: 0.86,
  GBP_USD: 1.33,
  USD_GBP: 0.75
};

/* eslint-disable react/prop-types */
function TransferModal({
  isOpen,
  sourceAccount,
  destinationAccounts,
  onClose,
  onSubmit,
  onNotify
}) {
  const [transferMode, setTransferMode] = useState('myAccounts');
  const [destinationAccountId, setDestinationAccountId] = useState('');
  const [destinationAccountNumber, setDestinationAccountNumber] = useState('');
  const [amount, setAmount] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const isSubmittingRef = useRef(false);

  useEffect(() => {
    if (!isOpen) {
      setTransferMode('myAccounts');
      setDestinationAccountId('');
      setDestinationAccountNumber('');
      setAmount('');
      setIsSubmitting(false);
      isSubmittingRef.current = false;
    }
  }, [isOpen]);

  if (!isOpen || !sourceAccount) {
    return null;
  }

  const formatMoney = (value, currency) => {
    try {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency
      }).format(value);
    } catch {
      return `${Number(value).toFixed(2)} ${currency}`;
    }
  };

  const getExchangeRate = (fromCurrency, toCurrency) => {
    if (fromCurrency === toCurrency) {
      return 1;
    }

    return exchangeRates[`${fromCurrency}_${toCurrency}`] ?? 1;
  };

  const availableDestinations = destinationAccounts.filter(
    (account) => account.id !== sourceAccount.id
  );
  const selectedDestinationAccount = availableDestinations.find(
    (account) => account.id === Number(destinationAccountId)
  );
  const parsedAmount = parseFloat(amount);
  const hasValidAmount = Number.isFinite(parsedAmount) && parsedAmount > 0;
  const exchangeRate = selectedDestinationAccount
    ? getExchangeRate(sourceAccount.currency, selectedDestinationAccount.currency)
    : null;
  const estimatedDestinationAmount =
    selectedDestinationAccount && hasValidAmount ? parsedAmount * exchangeRate : null;

  const handleSubmit = async (event) => {
    event.preventDefault();

    // Lock immediately so rapid double-clicks cannot queue a second transfer.
    if (isSubmittingRef.current) {
      return;
    }

    if (transferMode === 'myAccounts') {
      if (!destinationAccountId) {
        onNotify('Please choose a destination account.', 'error');
        return;
      }
    } else {
      const trimmedAccountNumber = destinationAccountNumber.trim().toUpperCase();

      if (!trimmedAccountNumber) {
        onNotify('Please enter the destination account number.', 'error');
        return;
      }

      if (trimmedAccountNumber === sourceAccount.accountNumber.toUpperCase()) {
        onNotify('You cannot transfer to the same account number.', 'error');
        return;
      }
    }

    if (!parsedAmount || parsedAmount <= 0) {
      onNotify('Please enter a valid transfer amount.', 'error');
      return;
    }

    isSubmittingRef.current = true;
    setIsSubmitting(true);

    try {
      const payload =
        transferMode === 'myAccounts'
          ? {
              fromAccountId: sourceAccount.id,
              toAccountId: Number(destinationAccountId),
              amount: parsedAmount
            }
          : {
              fromAccountId: sourceAccount.id,
              toAccounNumber: destinationAccountNumber.trim().toUpperCase(),
              amount: parsedAmount
            };

      const success = await onSubmit(payload);

      if (success) {
        onClose();
      }
    } finally {
      isSubmittingRef.current = false;
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 px-4">
      <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl">
        <div className="mb-5 flex items-start justify-between gap-4">
          <div>
            <h3 className="text-lg font-bold text-gray-900">Transfer Funds</h3>
            <p className="mt-1 text-sm text-gray-500">
              Source: <span className="font-semibold text-gray-700">{sourceAccount.accountNumber}</span>
            </p>
          </div>
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="text-gray-400 hover:text-gray-600 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Close
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="rounded-xl bg-slate-100 p-1">
            <div className="grid grid-cols-2 gap-1">
              <button
                type="button"
                onClick={() => setTransferMode('myAccounts')}
                disabled={isSubmitting}
                className={`rounded-lg px-3 py-2 text-sm font-semibold transition ${
                  transferMode === 'myAccounts'
                    ? 'bg-white text-slate-900 shadow-sm'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                My Accounts
              </button>
              <button
                type="button"
                onClick={() => setTransferMode('otherCustomer')}
                disabled={isSubmitting}
                className={`rounded-lg px-3 py-2 text-sm font-semibold transition ${
                  transferMode === 'otherCustomer'
                    ? 'bg-white text-slate-900 shadow-sm'
                    : 'text-slate-600 hover:text-slate-900'
                }`}
              >
                Other Customer
              </button>
            </div>
          </div>

          <div>
            <label className="mb-1 block text-sm font-semibold text-gray-700">
              {transferMode === 'myAccounts' ? 'Destination' : 'Account Number / IBAN'}
            </label>
            {transferMode === 'myAccounts' ? (
              <select
                value={destinationAccountId}
                onChange={(event) => setDestinationAccountId(event.target.value)}
                disabled={isSubmitting}
                className="w-full rounded border p-3 outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Select destination account</option>
                {availableDestinations.map((account) => (
                  <option key={account.id} value={account.id} disabled={!account.isActive}>
                    {account.accountNumber} · {account.accountType} ·{' '}
                    {!account.isActive ? 'Closed' : `${account.balance} ${account.currency}`}
                  </option>
                ))}
              </select>
            ) : (
              <input
                type="text"
                value={destinationAccountNumber}
                onChange={(event) => setDestinationAccountNumber(event.target.value)}
                disabled={isSubmitting}
                placeholder="Type destination account number"
                className="w-full rounded border p-3 uppercase outline-none focus:ring-2 focus:ring-blue-500"
              />
            )}
          </div>

          <div>
            <label className="mb-1 block text-sm font-semibold text-gray-700">Amount</label>
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={amount}
              onChange={(event) => setAmount(event.target.value)}
              disabled={isSubmitting}
              placeholder="Enter transfer amount"
              className="w-full rounded border p-3 outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {transferMode === 'myAccounts' && selectedDestinationAccount && hasValidAmount && (
            <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 text-sm text-amber-950">
              <p className="font-semibold">Estimated Conversion</p>
              <p className="mt-2">
                {formatMoney(parsedAmount, sourceAccount.currency)} from{' '}
                {sourceAccount.currency} at rate {exchangeRate} is estimated to arrive as{' '}
                <span className="font-bold">
                  {formatMoney(estimatedDestinationAmount, selectedDestinationAccount.currency)}
                </span>
                .
              </p>
            </div>
          )}

          {transferMode === 'otherCustomer' && hasValidAmount && (
            <div className="rounded-lg border border-slate-200 bg-slate-50 p-4 text-sm text-slate-700">
              The destination account will be resolved by account number when the teller submits
              the transfer. Exchange rate and destination currency will be determined by that
              account.
            </div>
          )}

          <div className="rounded-lg border border-blue-100 bg-blue-50 p-3 text-sm text-blue-900">
            Transfers can only be made between active accounts and cannot use the same
            account as both source and destination.
          </div>

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="flex-1 rounded border border-gray-200 px-4 py-3 text-sm font-semibold text-gray-700 hover:bg-gray-50"
            >
              {isSubmitting ? 'Transfer In Progress' : 'Cancel'}
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex-1 rounded bg-blue-900 px-4 py-3 text-sm font-semibold text-white hover:bg-blue-800 disabled:cursor-not-allowed disabled:opacity-70"
            >
              {isSubmitting ? 'Transferring...' : 'Transfer'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default TransferModal;
