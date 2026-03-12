using minibank.DTOs;
using minibank.Wrappers;
namespace minibank.Services.Interfaces
{
    public interface IAccountService
    {
        Task<ApiResponse<List<AccountDto>>> GetAccountByCustomerIdAsync(int customerId);
        Task<ApiResponse<AccountDto>> GetAccountDetailsAsync(int accountId);
        Task<ApiResponse<List<TransactionDto>>> GetAccountTransactionAsync(int accountId);
        Task<ApiResponse<TransactionDto>> CreateTransactionAsync(PostTransactionDto dto);
    }
}