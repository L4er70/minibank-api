import { useEffect, useState } from 'react';

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
  onSubmit
}) {
  const [destinationAccountId, setDestinationAccountId] = useState('');
  const [amount, setAmount] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (!isOpen) {
      setDestinationAccountId('');
      setAmount('');
      setIsSubmitting(false);
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

    if (!destinationAccountId) {
      alert('Please choose a destination account.');
      return;
    }

    if (!parsedAmount || parsedAmount <= 0) {
      alert('Please enter a valid transfer amount.');
      return;
    }

    setIsSubmitting(true);

    try {
      const success = await onSubmit({
        fromAccountId: sourceAccount.id,
        toAccountId: Number(destinationAccountId),
        amount: parsedAmount
      });

      if (success) {
        onClose();
      }
    } finally {
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
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            Close
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-semibold text-gray-700">Destination</label>
            <select
              value={destinationAccountId}
              onChange={(event) => setDestinationAccountId(event.target.value)}
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
          </div>

          <div>
            <label className="mb-1 block text-sm font-semibold text-gray-700">Amount</label>
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={amount}
              onChange={(event) => setAmount(event.target.value)}
              placeholder="Enter transfer amount"
              className="w-full rounded border p-3 outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {selectedDestinationAccount && hasValidAmount && (
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

          <div className="rounded-lg border border-blue-100 bg-blue-50 p-3 text-sm text-blue-900">
            Transfers can only be made between active accounts and cannot use the same
            account as both source and destination.
          </div>

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 rounded border border-gray-200 px-4 py-3 text-sm font-semibold text-gray-700 hover:bg-gray-50"
            >
              Cancel
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
