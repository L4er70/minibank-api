function AccountsPanel({ selectedCustomer, accounts, onClose }) {
  if (!selectedCustomer) return null;

  return (
    <div className="mt-12 bg-white p-6 rounded-lg shadow-inner border-t-4 border-blue-900 animate-fadeIn">
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-xl font-bold text-gray-800">
          Accounts for {selectedCustomer.firstName} {selectedCustomer.lastName}
        </h2>
        <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
          Close ✕
        </button>
      </div>

      {accounts.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {accounts.map((account) => (
            <div
              key={account.id}
              className="p-4 border rounded-lg bg-gray-50 flex justify-between items-center"
            >
              <div>
                <p className="text-xs text-gray-500 uppercase font-bold">{account.accountType}</p>
                <p className="text-lg font-mono">{account.accountNumber}</p>
              </div>
              <div className="text-right">
                <p className="text-sm text-gray-500">Balance</p>
                <p className="text-xl font-bold text-green-700">
                  ${account.balance.toLocaleString()}
                </p>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="text-center py-8 border-2 border-dashed rounded-lg">
          <p className="text-gray-500 mb-4">No accounts found for this customer.</p>
          <button className="bg-blue-900 text-white px-4 py-2 rounded text-sm">
            + Open New Account
          </button>
        </div>
      )}
    </div>
  );
}

export default AccountsPanel;
