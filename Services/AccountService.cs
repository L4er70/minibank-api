using minibank.Data;
using minibank.DTOs;
using minibank.Models;
using minibank.Services.Interfaces;
using minibank.Wrappers;
using Microsoft.EntityFrameworkCore;
using minibank.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace minibank.Services
{
    public class AccountService : IAccountService
    {
        private readonly BankingDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        public AccountService(BankingDbContext context,IHttpClientFactory httpClientFactory,IMemoryCache cache)

        {

            _context = context;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        private async Task<decimal> GetLiveExchangeRate(string from,string to)
        {
            if(from==to)return 1.0m;

            string cacheKey = $"rate_{from}_{to}";

            if(_cache.TryGetValue(cacheKey,out decimal cachedRate))
            {
                return cachedRate;
            }
            decimal liveRate = 1.0m;

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            try
            {
                var response = await client.GetFromJsonAsync<ExchangeResponse>
                ($"https://api.frankfurter.app/latest?from={from}&to={to}");

                if(response!=null && response.Rates.ContainsKey(to))
                {
                    liveRate = response.Rates[to];
                }
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Exchange rate service timed out.Please try again.");
            }
            catch(Exception)
            {
               // throw new Exception("Unable to fetch live exchange rates.Transfer aborted.");
                return GetExchangeRate(from,to);
            }
            var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetSlidingExpiration(TimeSpan.FromMinutes(10));

            _cache.Set(cacheKey,liveRate,cacheEntryOptions);
            return liveRate;
        }
        public class ExchangeResponse
        {
            public string Base{get;set;}
            public Dictionary<string,decimal>Rates{get;set;}
        }
        
        private decimal GetExchangeRate(string fromCurrency,string toCurrency)
        {
            if(fromCurrency == toCurrency)return 1.0m;

            var rates = new Dictionary<string, decimal>
            {
                {"EUR_ALL",103.50m},
                {"ALL_EUR",0.0097m},
                {"USD_ALL",95.20m},
                {"ALL_USD",0.105m},
                {"EUR_USD",1.08m},
                {"USD_EUR",0.92m},
                {"GBP_ALL",111.01m},
                {"ALL_GBP",0.0090m},
                {"GBP_EUR",1.16m},
                {"EUR_GBP",0.86m},
                {"GBP_USD",1.33m},
                {"USD_GBP",0.75m}

            };
            string key = $"{fromCurrency}_{toCurrency}";
            return rates.ContainsKey(key) ? rates[key] : 1.0m;
        }  

        public async Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountDto dto)
        {
           var customerExists = await _context.Customers.AnyAsync(
            c => c.Id == dto.CustomerId
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
               CreatedAt = newAccount.CreatedAt,
               IsActive = newAccount.IsActive
           });
           // throw new NotImplementedException();
        }

        public async Task<ApiResponse<TransactionDto>> CreateTransactionAsync(PostTransactionDto dto)
        {
            if (dto.Amount <= 0)return ApiResponse<TransactionDto>.FailureResponse("Amount must be greater than zero.");
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? dbTransaction = null;

            try{
            // Load account to validate existence and update balance.
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == dto.AccountId);

            if (account == null)
            {
                return ApiResponse<TransactionDto>.FailureResponse("Account not found.");
            }

            if (!account.IsActive)
            {
                return ApiResponse<TransactionDto>.FailureResponse("Closed accounts cannot accept transactions.");
            }

            if (dto.TransactionType == TransactionType.Debit && account.Balance < dto.Amount)
            {
                return ApiResponse<TransactionDto>.FailureResponse("Insufficient funds.");
            }

            dbTransaction = await _context.Database.BeginTransactionAsync();

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
                if (dbTransaction != null)
                {
                    await dbTransaction.RollbackAsync();
                }
                return ApiResponse<TransactionDto>.FailureResponse("A database error occured. Money was not moved.");
            }
            finally
            {
                if (dbTransaction != null)
                {
                    await dbTransaction.DisposeAsync();
                }
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
                CreatedAt = a.CreatedAt,
                IsActive = a.IsActive
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
                    CreatedAt = a.CreatedAt,
                    IsActive = a.IsActive
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

        public async Task<ApiResponse<bool>> CloseAccountAsync(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if(account == null) return ApiResponse<bool>.FailureResponse("Account not found.");

            if (account.Balance != 0)
            {
                return ApiResponse<bool>.FailureResponse("Account balance must be zero before closing.");

            }
            account.IsActive = false;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true,"Account closed successfully.");
        }
        public async Task<ApiResponse<bool>> ReopenAccountAsync(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if(account == null)return ApiResponse<bool>.FailureResponse("Account not found.");

            if(account.IsActive)return ApiResponse<bool>.FailureResponse("Account is already active.");

            account.IsActive=true;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true,"Account re-opened successfully.");
            
        }

        public async Task<ApiResponse<bool>> TransferAsync(TransferDto dto)
        {
            if(dto.FromAccountId == dto.ToAccountId)
            {
                return ApiResponse<bool>.FailureResponse("Cannot transfer money to the same account.");
            }
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var fromAccount = await _context.Accounts.FindAsync(dto.FromAccountId);
                var toAccount = await _context.Accounts.FindAsync(dto.ToAccountId);

                if(fromAccount == null || toAccount == null)
                return ApiResponse<bool>.FailureResponse("One or both accounts could not be found.");

                if(!fromAccount.IsActive || !toAccount.IsActive)
                return ApiResponse<bool>.FailureResponse("Transfers cannot involve closed or inactive accounts.");

                if(fromAccount.Balance < dto.Amount)
                return ApiResponse<bool>.FailureResponse("Insufficent funds for this transfer.");

                decimal exchangeRate = await GetLiveExchangeRate(fromAccount.Currency.ToString(),toAccount.Currency.ToString());
                decimal destinationAmount = dto.Amount * exchangeRate;

                fromAccount.Balance-=dto.Amount;
                toAccount.Balance+=destinationAmount;

                var debitDesc = $"Transfer OUT:{dto.Amount} {fromAccount.Currency} ->{toAccount.Currency}(Rate:{exchangeRate})";
                var creditDesc = $"Transfer IN: Received {destinationAmount}{toAccount.Currency} from {fromAccount.AccountNumber}";

                var debitTransaction = new Transaction
                {
                    AccountId = dto.FromAccountId,
                    Amount = dto.Amount,
                    Type = minibank.Enums.TransactionType.Debit,
                    TransactionDate = DateTime.UtcNow,
                    Description = debitDesc
                };

                var creditTransaction = new Transaction
                {
                    AccountId = dto.ToAccountId,
                    Amount =destinationAmount,
                    Type = minibank.Enums.TransactionType.Credit,
                    TransactionDate = DateTime.UtcNow,
                    Description = creditDesc
                };

                _context.Transactions.Add(debitTransaction);
                _context.Transactions.Add(creditTransaction);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true,"Transfer completed successfully.");

            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();

                return ApiResponse<bool>.FailureResponse("A system error occured during the transfer. No funds were moved.");

                
            }


            //throw new NotImplementedException();
        }
    }
}
