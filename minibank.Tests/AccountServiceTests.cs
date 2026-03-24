using System.Net;
using System.Net.Http.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using minibank.Data;
using minibank.DTOs;
using minibank.Enums;
using minibank.Models;
using minibank.Services;
using Moq;

namespace minibank.Tests
{
    public class AccountServiceTests
    {
        [Fact]
        public async Task CreateTransactionAsync_DebitExceedsBalance_ReturnsInsufficientFunds()
        {
            await using var fixture = await AccountServiceFixture.CreateAsync();
            fixture.Context.Customers.Add(new Customer { Id = 1, PersonalId = "P001", FirstName = "Test", LastName = "Owner", DateOfBirth = new DateTime(1990, 1, 1) });
            fixture.Context.Accounts.Add(new Account
            {
                Id = 1,
                AccountNumber = "AL001",
                Balance = 100m,
                Currency = Currency.ALL,
                IsActive = true,
                CustomerId = 1
            });
            await fixture.Context.SaveChangesAsync();

            var service = fixture.CreateService();
            var dto = new PostTransactionDto
            {
                AccountId = 1,
                Amount = 150m,
                TransactionType = TransactionType.Debit,
                Description = "ATM withdrawal"
            };

            // Act
            var response = await service.CreateTransactionAsync(dto);

            Assert.False(response.Success);
            Assert.Equal("Insufficient funds.", response.Message);

            var account = await fixture.Context.Accounts.SingleAsync();
            Assert.Equal(100m, account.Balance);
        }

        [Fact]
        public async Task CreateTransactionAsync_ClosedAccount_ReturnsFailure()
        {
            await using var fixture = await AccountServiceFixture.CreateAsync();
            fixture.Context.Customers.Add(new Customer { Id = 1, PersonalId = "P002", FirstName = "Test", LastName = "Owner", DateOfBirth = new DateTime(1990, 1, 1) });
            fixture.Context.Accounts.Add(new Account
            {
                Id = 1,
                AccountNumber = "AL002",
                Balance = 100m,
                Currency = Currency.ALL,
                IsActive = false,
                CustomerId = 1
            });
            await fixture.Context.SaveChangesAsync();

            var service = fixture.CreateService();
            var dto = new PostTransactionDto
            {
                AccountId = 1,
                Amount = 50m,
                TransactionType = TransactionType.Credit,
                Description = "Teller deposit"
            };

            // Act
            var response = await service.CreateTransactionAsync(dto);

            Assert.False(response.Success);
            Assert.Equal("Closed accounts cannot accept transactions.", response.Message);

            var account = await fixture.Context.Accounts.SingleAsync();
            Assert.Equal(100m, account.Balance);
        }

        [Fact]
        public async Task TransferAsync_SameCurrency_TransfersWithoutCallingExchangeApi()
        {
            await using var fixture = await AccountServiceFixture.CreateAsync();
            fixture.Context.Customers.AddRange(
                new Customer { Id = 1, PersonalId = "P003", FirstName = "One", LastName = "Owner", DateOfBirth = new DateTime(1990, 1, 1) },
                new Customer { Id = 2, PersonalId = "P004", FirstName = "Two", LastName = "Owner", DateOfBirth = new DateTime(1990, 1, 1) });
            fixture.Context.Accounts.AddRange(
                new Account { Id = 1, AccountNumber = "AL100", Balance = 200m, Currency = Currency.EUR, IsActive = true, CustomerId = 1 },
                new Account { Id = 2, AccountNumber = "AL200", Balance = 50m, Currency = Currency.EUR, IsActive = true, CustomerId = 2 });
            await fixture.Context.SaveChangesAsync();

            var service = fixture.CreateService();
            var response = await service.TransferAsync(new TransferDto
            {
                FromAccountId = 1,
                ToAccountId = 2,
                Amount = 25m
            });

            Assert.True(response.Success);

            var from = await fixture.Context.Accounts.SingleAsync(a => a.Id == 1);
            var to = await fixture.Context.Accounts.SingleAsync(a => a.Id == 2);
            Assert.Equal(175m, from.Balance);
            Assert.Equal(75m, to.Balance);
            fixture.HttpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_CrossCurrency_UsesLiveExchangeRate()
        {
            await using var fixture = await AccountServiceFixture.CreateAsync(
                """
                {"amount":1.0,"base":"EUR","date":"2026-03-24","rates":{"USD":1.08}}
                """);
            fixture.Context.Customers.AddRange(
                new Customer { Id = 1, PersonalId = "P005", FirstName = "Euro", LastName = "Owner", DateOfBirth = new DateTime(1990, 1, 1) },
                new Customer { Id = 2, PersonalId = "P006", FirstName = "Dollar", LastName = "Owner", DateOfBirth = new DateTime(1990, 1, 1) });
            fixture.Context.Accounts.AddRange(
                new Account { Id = 1, AccountNumber = "AL300", Balance = 150m, Currency = Currency.EUR, IsActive = true, CustomerId = 1 },
                new Account { Id = 2, AccountNumber = "AL400", Balance = 10m, Currency = Currency.USD, IsActive = true, CustomerId = 2 });
            await fixture.Context.SaveChangesAsync();

            var service = fixture.CreateService();
            var response = await service.TransferAsync(new TransferDto
            {
                FromAccountId = 1,
                ToAccountId = 2,
                Amount = 50m
            });

            Assert.True(response.Success);

            var from = await fixture.Context.Accounts.SingleAsync(a => a.Id == 1);
            var to = await fixture.Context.Accounts.SingleAsync(a => a.Id == 2);
            Assert.Equal(100m, from.Balance);
            Assert.Equal(64m, to.Balance);

            var transactions = await fixture.Context.Transactions.OrderBy(t => t.Id).ToListAsync();
            Assert.Equal(2, transactions.Count);
            Assert.Contains("Rate:1.08", transactions[0].Description);
            Assert.Contains("54.00USD", transactions[1].Description);
        }

        [Fact]
        public async Task TransferAsync_WhenExchangeApiFails_ReturnsFailureAndDoesNotMoveFunds()
        {
            await using var fixture = await AccountServiceFixture.CreateAsync(handler: new ThrowingHandler());
            fixture.Context.Customers.AddRange(
                new Customer { Id = 1, PersonalId = "P007", FirstName = "Pound", LastName = "Owner", DateOfBirth = new DateTime(1990, 1, 1) },
                new Customer { Id = 2, PersonalId = "P008", FirstName = "Usd", LastName = "Owner", DateOfBirth = new DateTime(1990, 1, 1) });
            fixture.Context.Accounts.AddRange(
                new Account { Id = 1, AccountNumber = "AL500", Balance = 80m, Currency = Currency.GBP, IsActive = true, CustomerId = 1 },
                new Account { Id = 2, AccountNumber = "AL600", Balance = 20m, Currency = Currency.USD, IsActive = true, CustomerId = 2 });
            await fixture.Context.SaveChangesAsync();

            var service = fixture.CreateService();
            var response = await service.TransferAsync(new TransferDto
            {
                FromAccountId = 1,
                ToAccountId = 2,
                Amount = 10m
            });

            Assert.False(response.Success);
            Assert.Equal("A system error occured during the transfer. No funds were moved.", response.Message);

            var from = await fixture.Context.Accounts.SingleAsync(a => a.Id == 1);
            var to = await fixture.Context.Accounts.SingleAsync(a => a.Id == 2);
            Assert.Equal(80m, from.Balance);
            Assert.Equal(20m, to.Balance);
            Assert.Empty(await fixture.Context.Transactions.ToListAsync());
        }

        private sealed class AccountServiceFixture : IAsyncDisposable
        {
            private readonly SqliteConnection _connection;

            private AccountServiceFixture(BankingDbContext context, SqliteConnection connection, Mock<IHttpClientFactory> httpClientFactoryMock)
            {
                Context = context;
                _connection = connection;
                HttpClientFactoryMock = httpClientFactoryMock;
            }

            public BankingDbContext Context { get; }
            public Mock<IHttpClientFactory> HttpClientFactoryMock { get; }

            public static async Task<AccountServiceFixture> CreateAsync(string? jsonResponse = null, HttpMessageHandler? handler = null)
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                await connection.OpenAsync();

                var options = new DbContextOptionsBuilder<BankingDbContext>()
                    .UseSqlite(connection)
                    .Options;

                var context = new BankingDbContext(options);
                await context.Database.EnsureCreatedAsync();

                handler ??= new StubHttpMessageHandler(jsonResponse ?? """{"rates":{"USD":1.0}}""");
                var client = new HttpClient(handler);
                var factory = new Mock<IHttpClientFactory>();
                factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

                return new AccountServiceFixture(context, connection, factory);
            }

            public AccountService CreateService() => new(Context, HttpClientFactoryMock.Object);

            public async ValueTask DisposeAsync()
            {
                await Context.DisposeAsync();
                await _connection.DisposeAsync();
            }
        }

        private sealed class StubHttpMessageHandler(string jsonResponse) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(System.Text.Json.JsonSerializer.Deserialize<object>(jsonResponse))
                });
            }
        }

        private sealed class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("boom");
            }
        }
    }
}
