using minibank.Enums;
using minibank.Models;
using Microsoft.Extensions.DependencyInjection;

namespace minibank.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = serviceProvider.GetRequiredService<BankingDbContext>();

            context.Database.EnsureCreated();

            if (context.Customers.Any() || context.Accounts.Any() || context.Transactions.Any())
            {
                return;
            }

            var now = DateTime.UtcNow;

            var customers = new List<Customer>
            {
                new Customer
                {
                    PersonalId = "PID1000001",
                    FirstName = "Jane",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1990, 4, 12),
                    CreatedAt = now.AddMonths(-6),
                    Accounts = new List<Account>
                    {
                        new Account
                        {
                            AccountNumber = "ALB0000000001",
                            Currency = Currency.ALL,
                            AccountType = AccountType.Current,
                            BranchCode = "TRN01",
                            CreatedAt = now.AddMonths(-5),
                            Balance = 6075m,
                            Transactions = new List<Transaction>
                            {
                                new Transaction
                                {
                                    Amount = 5000m,
                                    Type = TransactionType.Credit,
                                    Description = "Initial deposit",
                                    TransactionDate = now.AddMonths(-5)
                                },
                                new Transaction
                                {
                                    Amount = 125m,
                                    Type = TransactionType.Debit,
                                    Description = "Utility payment",
                                    TransactionDate = now.AddMonths(-3)
                                },
                                new Transaction
                                {
                                    Amount = 1200m,
                                    Type = TransactionType.Credit,
                                    Description = "Salary",
                                    TransactionDate = now.AddMonths(-1)
                                }
                            }
                        },
                        new Account
                        {
                            AccountNumber = "ALB0000000002",
                            Currency = Currency.USD,
                            AccountType = AccountType.Savings,
                            BranchCode = "TRN01",
                            CreatedAt = now.AddMonths(-4),
                            Balance = 2500m,
                            Transactions = new List<Transaction>
                            {
                                new Transaction
                                {
                                    Amount = 2500m,
                                    Type = TransactionType.Credit,
                                    Description = "Savings transfer",
                                    TransactionDate = now.AddMonths(-4)
                                }
                            }
                        }
                    }
                },
                new Customer
                {
                    PersonalId = "PID1000002",
                    FirstName = "Arben",
                    LastName = "Kola",
                    DateOfBirth = new DateTime(1985, 9, 3),
                    CreatedAt = now.AddMonths(-8),
                    Accounts = new List<Account>
                    {
                        new Account
                        {
                            AccountNumber = "ALB0000000003",
                            Currency = Currency.EUR,
                            AccountType = AccountType.Current,
                            BranchCode = "DRR02",
                            CreatedAt = now.AddMonths(-7),
                            Balance = 1225m,
                            Transactions = new List<Transaction>
                            {
                                new Transaction
                                {
                                    Amount = 1500m,
                                    Type = TransactionType.Credit,
                                    Description = "Initial deposit",
                                    TransactionDate = now.AddMonths(-7)
                                },
                                new Transaction
                                {
                                    Amount = 200m,
                                    Type = TransactionType.Debit,
                                    Description = "ATM withdrawal",
                                    TransactionDate = now.AddMonths(-6)
                                },
                                new Transaction
                                {
                                    Amount = 75m,
                                    Type = TransactionType.Debit,
                                    Description = "Card purchase",
                                    TransactionDate = now.AddMonths(-2)
                                }
                            }
                        },
                        new Account
                        {
                            AccountNumber = "ALB0000000004",
                            Currency = Currency.GBP,
                            AccountType = AccountType.Savings,
                            BranchCode = "DRR02",
                            CreatedAt = now.AddMonths(-3),
                            Balance = 800m,
                            Transactions = new List<Transaction>
                            {
                                new Transaction
                                {
                                    Amount = 800m,
                                    Type = TransactionType.Credit,
                                    Description = "Gift",
                                    TransactionDate = now.AddMonths(-3)
                                }
                            }
                        }
                    }
                }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}
