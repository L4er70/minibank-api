import { useState, useEffect } from 'react';
import axios from 'axios';

// 1. Configure our API connection
const api = axios.create({
  baseURL: 'http://localhost:5272/api', // <-- Check your .NET terminal for this port
});

function App() {
  // 2. Set up "State" to hold our data
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);

  // 3. Fetch data when the page loads
  useEffect(() => {
    api.get('/Customers')
      .then(response => {
        // Because we built that nice ApiResponse wrapper in .NET, the data is inside .data.data
        setCustomers(response.data.data || []);
        setLoading(false);
      })
      .catch(error => {
        console.error("Error fetching data:", error);
        setLoading(false);
      });
  }, []);

  // 4. The UI (HTML + Tailwind CSS)
  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold text-blue-900 mb-6">Customer Directory</h1>

        {loading ? (
          <p className="text-gray-500 animate-pulse">Loading secure data...</p>
        ) : (
          <div className="bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-100">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">ID</th>
                  <th className="px-6 py-3 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">Client Name</th>
                  <th className="px-6 py-3 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">Status</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {customers.map((customer) => (
                  <tr key={customer.id} className="hover:bg-blue-50 transition duration-150">
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">#{customer.id}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {customer.firstName} {customer.lastName}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800">
                        Active
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            
            {/* Show this if the database is empty */}
            {customers.length === 0 && (
              <div className="p-6 text-center text-gray-500">
                No customers found. Time to add some!
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export default App;