using minibank.DTOs;
using minibank.Models;
using minibank.Wrappers;

namespace minibank.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ApiResponse<Customer>> RegisterCustomerAsync(CreateCustomerDto dto);
    }
}