function SearchBar({ value, onChange }) {
  return (
    <div className="mb-4 relative">
      <input
        type="text"
        placeholder="Search by name or Personal ID..."
        value={value}
        onChange={onChange}
        className="w-full p-3 pl-10 border rounded-lg shadow-sm focus:ring-2 focus:ring-blue-500 outline-none"
      />
      <span className="absolute left-3 top-3.5 text-gray-400">🔍</span>
    </div>
  );
}

export default SearchBar;
