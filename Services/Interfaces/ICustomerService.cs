using minibank.DTOs;
using minibank.Models;
using minibank.Wrappers;

namespace minibank.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ApiResponse<Customer>> RegisterCustomerAsync(CreateCustomerDto dto);
        Task<ApiResponse<List<Customer>>> GetAllCustomersAsync();
        Task<ApiResponse<List<Customer>>> SearchCustomersAsync(string qurey);
        Task<ApiResponse<Customer>> GetCustomerByIdAsync(int id);
    }
}