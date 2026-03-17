import { useState, useEffect } from 'react';
import api from './api/axios';
import AccountsPanel from './components/AccountsPanel';
import CustomerTable from './components/CustomerTable';
import Header from './components/Header';
import RegisterForm from './components/RegisterForm';
import SearchBar from './components/SearchBar';
import SuccessToast from './components/SuccessToast';

function App() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showSuccess, setShowSuccess] = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [accounts, setAccounts] = useState([]);
  const [searchQuery, setSearchQuery] = useState('');
  
  
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

  //filter
  const filteredCustomers = customers.filter((customer) =>
    customer.firstName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    customer.lastName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    customer.personalId.toLowerCase().includes(searchQuery.toLowerCase())
  );





  //handle view account
  const handleViewAccounts = async (customer) => {
    setSelectedCustomer(customer);
    try {
      const response = await api.get(`/Account/customer/${customer.id}`);
      setAccounts(response.data.data || []);
    } catch (error) {
      console.error("Error fetching accounts: ", error);
      setAccounts([]);
    }
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
        setTimeout(() => setShowSuccess(false), 3000);
      }
    } catch (error) {
      alert("Registration failed. Check if Personal ID already exists.");
      console.error(error);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 p-8 flex justify-center">
      <SuccessToast show={showSuccess} message="✅ Customer Registered Successfully!" />
      <div className="w-full max-w-6xl">
        <Header />
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
          onClose={() => setSelectedCustomer(null)}
        />
      </div>
    </div>
  );
}

export default App;
