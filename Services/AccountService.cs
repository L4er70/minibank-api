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

        public async Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountDto dto)
        {
           var customerExists = await _context.Customers.AnyAsync(
            c=>c.Id == dto.CustomerId
           );
           if(!customerExists) return ApiResponse<AccountDto>.FailureResponse("Customer does not exists");

           string newIban = $"AL{new Random().Next(10,99)}BKT777{new Random().Next(100000,999999)}";


           var newAccount = new Account
           {
               CustomerId = dto.CustomerId,
               AccountNumber = newIban,
               Balance=0,
               Currency =dto.currency,
               BranchCode = dto.BranchCode,
               CreatedAt = DateTime.UtcNow
           };

           _context.Accounts.Add(newAccount);
           await _context.SaveChangesAsync();

           return ApiResponse<AccountDto>.SuccessResponse(new AccountDto
           {
               Id = newAccount.Id,
               AccountNumber = newAccount.AccountNumber,
               Balance = newAccount.Balance,
               Currency = newAccount.Currency.ToString(),
               AccountType = newAccount.AccountType.ToString(),
               CreatedAt = newAccount.CreatedAt
           });
           // throw new NotImplementedException();
        }

        public async Task<ApiResponse<TransactionDto>> CreateTransactionAsync(PostTransactionDto dto)
        {
            if (dto.Amount <= 0)return ApiResponse<TransactionDto>.FailureResponse("Amount must be greater than zero.");

            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try{
            // Load account to validate existence and update balance.
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

            // Create transaction and apply balance change in one unit of work.
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

            await dbTransaction.CommitAsync();

            // Map to DTO for API response.
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
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                return ApiResponse<TransactionDto>.FailureResponse("A database error occured. Money was not moved.");
            }
        }

        public async Task<ApiResponse<List<AccountDto>>> GetAccountByCustomerIdAsync(int customerId)
        {
            // Project to DTO to avoid loading full entities.
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
            // Fetch a single account summary by id.
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
            // Return all transactions for the given account.
            var transactions =await _context.Transactions
            .Where(t=>t.AccountId==accountId)
            .OrderByDescending(t=>t.TransactionDate)
            .Select(t=> new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Description = t.Description,
                TransactionDate = t.TransactionDate
            }).ToListAsync();

            if (!transactions.Any())
            {
                return ApiResponse<List<TransactionDto>>.FailureResponse("No transaction found for this account.");
            }

            return ApiResponse<List<TransactionDto>>.SuccessResponse(transactions,"Transactions retrieved.");
            //throw new NotImplementedException();
        }
    }
}
