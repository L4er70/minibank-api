function CustomerTable({ customers, onViewAccounts }) {
  return (
    <div className="bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-100">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-bold text-gray-600 uppercase">ID</th>
            <th className="px-6 py-3 text-left text-xs font-bold text-gray-600 uppercase">Full Name</th>
            <th className="px-6 py-3 text-left text-xs font-bold text-gray-600 uppercase">ID Number</th>
            <th className="px-6 py-3 text-left text-xs font-bold text-gray-600 uppercase">Actions</th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {customers.map((customer) => (
            <tr key={customer.id} className="hover:bg-blue-50 transition">
              <td className="px-6 py-4 text-sm text-gray-500">#{customer.id}</td>
              <td className="px-6 py-4 text-sm font-medium text-gray-900">
                {customer.firstName} {customer.lastName}
              </td>
              <td className="px-6 py-4 text-sm text-gray-600 font-mono">{customer.personalId}</td>
              <td className="px-6 py-4 text-sm">
                <button
                  onClick={() => onViewAccounts(customer)}
                  className="text-blue-600 hover:text-blue-900 font-semibold"
                >
                  View Accounts
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default CustomerTable;
