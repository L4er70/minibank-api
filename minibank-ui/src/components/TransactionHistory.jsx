import React from 'react';

const TransactionHistory = ({ transactions }) => {
  if (!transactions || transactions.length === 0) {
    return <p className="text-gray-400 text-xs italic p-4">No recent activity for this account.</p>;
  }

  return (
    <div className="mt-4 overflow-hidden rounded-lg border border-gray-100 bg-white shadow-sm">
      <table className="w-full text-left text-xs">
        <thead className="bg-gray-50 text-gray-500 font-bold uppercase tracking-wider">
          <tr>
            <th className="p-3">Date</th>
            <th className="p-3">Description</th>
            <th className="p-3 text-right">Amount</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {transactions.map((t) => (
            <tr key={t.id} className="hover:bg-blue-50/50 transition-colors">
              <td className="p-3 text-gray-600">
                {new Date(t.transactionDate).toLocaleDateString()}
              </td>
              <td className="p-3 text-gray-700 font-medium">{t.description}</td>
              <td className={`p-3 text-right font-bold ${t.type === 'Credit' ? 'text-green-600' : 'text-red-600'}`}>
                {t.type === 'Credit' ? '+' : '-'}${t.amount.toLocaleString()}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default TransactionHistory;