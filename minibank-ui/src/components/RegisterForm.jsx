function RegisterForm({ formData, onChange, onSubmit }) {
  return (
    <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 mb-8">
      <h2 className="text-xl font-semibold mb-4 text-gray-800">Register New Customer</h2>
      <form onSubmit={onSubmit} className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <input
          type="text"
          name="firstName"
          placeholder="First Name"
          value={formData.firstName}
          onChange={onChange}
          required
          className="p-2 border rounded focus:ring-2 focus:ring-blue-500 outline-none"
        />
        <input
          type="text"
          name="lastName"
          placeholder="Last Name"
          value={formData.lastName}
          onChange={onChange}
          required
          className="p-2 border rounded focus:ring-2 focus:ring-blue-500 outline-none"
        />
        <input
          type="text"
          name="personalId"
          placeholder="Personal ID (e.g. J1234567L)"
          value={formData.personalId}
          onChange={onChange}
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
  );
}

export default RegisterForm;
