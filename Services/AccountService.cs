using minibank.Data;
using minibank.DTOs;
using minibank.Services.Interfaces;
using minibank.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace minibank.Services
{
    public class AccountService : IAccountService
    {
        private readonly BankingDbContext _context;
        public AccountService(BankingDbContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse<List<AccountDto>>> GetAccountByCustomerIdAsync(int customerId)
        {
            var accounts = await _context.Accounts
            .Where(a=>a.CustomerId == customerId)
            .Select(a=>new AccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                Currency = a.Currency.ToString(),
                AccountType=a.AccountType.ToString(),
                BranchCode = a.BranchCode,
                CreatedAt = a.CreatedAt
            }).ToListAsync();
            return ApiResponse<List<AccountDto>>.SuccessResponse(accounts);
            //throw new NotImplementedException();
        }

        public async Task<ApiResponse<AccountDto>> GetAccountDetailsAsync(int accountId)
        {
            var account =await _context.Accounts
            .Where(a=>a.Id == accountId).Select(
                a=>new AccountDto
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    Currency = a.Currency.ToString(),
                    AccountType = a.AccountType.ToString(),
                    BranchCode = a.BranchCode,
                    CreatedAt = a.CreatedAt
                }
            ).FirstOrDefaultAsync();

            if(account == null)return ApiResponse<AccountDto>.FailureResponse("Account not found.");

            return ApiResponse<AccountDto>.SuccessResponse(account);
            //throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<TransactionDto>>> GetAccountTransactionAsync(int accountId)
        {
            var transactions =await _context.Transactions
            .Where(t=>t.AccountId==accountId)
            .Select(t=> new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Description = t.Description,
                TransactionDate = t.TransactionDate
            }).ToListAsync();

            return ApiResponse<List<TransactionDto>>.SuccessResponse(transactions);
            //throw new NotImplementedException();
        }
    }
}