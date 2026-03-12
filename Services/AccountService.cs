using minibank.Data;
using minibank.DTOs;
using minibank.Models;
using minibank.Services.Interfaces;
using minibank.Wrappers;
using Microsoft.EntityFrameworkCore;
using minibank.Enums;

namespace minibank.Services
{
    public class AccountService : IAccountService
    {
        private readonly BankingDbContext _context;
        public AccountService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<TransactionDto>> CreateTransactionAsync(PostTransactionDto dto)
        {
            if (dto.Amount <= 0)
            {
                return ApiResponse<TransactionDto>.FailureResponse("Amount must be greater than zero.");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == dto.AccountId);

            if (account == null)
            {
                return ApiResponse<TransactionDto>.FailureResponse("Account not found.");
            }

            if (dto.TransactionType == TransactionType.Debit && account.Balance < dto.Amount)
            {
                return ApiResponse<TransactionDto>.FailureResponse("Insufficient funds.");
            }

            var transaction = new Transaction
            {
                AccountId = account.Id,
                Amount = dto.Amount,
                Type = dto.TransactionType,
                Description = dto.Description ?? string.Empty,
                TransactionDate = DateTime.UtcNow
            };

            account.Balance = dto.TransactionType == TransactionType.Credit
                ? account.Balance + dto.Amount
                : account.Balance - dto.Amount;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var result = new TransactionDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Type = transaction.Type.ToString(),
                Description = transaction.Description,
                TransactionDate = transaction.TransactionDate
            };

            return ApiResponse<TransactionDto>.SuccessResponse(result, "Transaction created.");
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
