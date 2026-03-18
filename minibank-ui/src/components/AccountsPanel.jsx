/* eslint-disable react/prop-types */
function AccountsPanel({
  selectedCustomer,
  accounts,
  amounts,
  onAmountChange,
  onTransaction,
  onClose
}) {
  if (!selectedCustomer) return null;

  return (
    <div className="mt-12 bg-white p-6 rounded-lg shadow-inner border-t-4 border-blue-900 animate-fadeIn">
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
          {accounts.map((account) => (
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
                    ${account.balance.toLocaleString()}
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
                    onClick={() => onTransaction(account.id, 'Deposit')}
                    className="flex-1 rounded bg-green-600 px-4 py-2 text-sm font-semibold text-white hover:bg-green-700"
                  >
                    Deposit
                  </button>
                  <button
                    onClick={() => onTransaction(account.id, 'Withdraw')}
                    className="flex-1 rounded bg-red-600 px-4 py-2 text-sm font-semibold text-white hover:bg-red-700"
                  >
                    Withdraw
                  </button>
                </div>
              </div>
            </div>
          ))}
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
