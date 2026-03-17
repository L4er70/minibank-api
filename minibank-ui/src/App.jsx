import { useState, useEffect } from 'react';
import api from './api/axios';

function App() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const[showSuccess,setShowSuccess] = useState(false);
  
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
      setLoading(false);
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

  // 3. Submit to Backend
  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await api.post('/customers', formData);
      if (response.data.success) {
        // Clear form and refresh list
        setFormData({ firstName: '', lastName: '', personalId: '' });
        fetchCustomers(); 
        //alert("Customer registered successfully!");
        setShowSuccess(true);
        setTimeout(()=> setShowSuccess(false),3000)
      }
    } catch (error) {
      alert("Registration failed. Check if Personal ID already exists.");
      console.error(error);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      {showSuccess && (
  <div className="fixed top-5 right-5 bg-green-600 text-white px-6 py-3 rounded-lg shadow-2xl animate-bounce z-50">
    ✅ Customer Registered Successfully!
  </div>
)}
      <div className="max-w-4xl mx-auto">
        <header className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold text-blue-900">Banking Portal</h1>
          <div className="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm font-medium">
            System Online
          </div>
        </header>

        {/* --- REGISTER FORM --- */}
        <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 mb-8">
          <h2 className="text-xl font-semibold mb-4 text-gray-800">Register New Customer</h2>
          <form onSubmit={handleSubmit} className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <input
              type="text"
              name="firstName"
              placeholder="First Name"
              value={formData.firstName}
              onChange={handleChange}
              required
              className="p-2 border rounded focus:ring-2 focus:ring-blue-500 outline-none"
            />
            <input
              type="text"
              name="lastName"
              placeholder="Last Name"
              value={formData.lastName}
              onChange={handleChange}
              required
              className="p-2 border rounded focus:ring-2 focus:ring-blue-500 outline-none"
            />
            <input
              type="text"
              name="personalId"
              placeholder="Personal ID (e.g. J1234567L)"
              value={formData.personalId}
              onChange={handleChange}
              required
              className="p-2 border rounded focus:ring-2 focus:ring-blue-500 outline-none"
            />
            <button 
              type="submit" 
              className="md:col-span-3 bg-blue-900 text-white py-2 rounded font-semibold hover:bg-blue-800 transition shadow-md"
            >
              Add Customer to Core Banking
            </button>
          </form>
        </div>

        {/* --- CUSTOMER TABLE --- */}
        <div className="bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-100">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-bold text-gray-600 uppercase">ID</th>
                <th className="px-6 py-3 text-left text-xs font-bold text-gray-600 uppercase">Full Name</th>
                <th className="px-6 py-3 text-left text-xs font-bold text-gray-600 uppercase">ID Number</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {customers.map((c) => (
                <tr key={c.id} className="hover:bg-blue-50 transition">
                  <td className="px-6 py-4 text-sm text-gray-500">#{c.id}</td>
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">{c.firstName} {c.lastName}</td>
                  <td className="px-6 py-4 text-sm text-gray-600 font-mono">{c.personalId}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default App;