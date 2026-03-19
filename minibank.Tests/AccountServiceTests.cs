using Microsoft.EntityFrameworkCore;
using minibank.Data;
using minibank.DTOs;
using minibank.Enums;
using minibank.Models;
using minibank.Services;
using minibank.Tests.Helpers;
using Moq;

namespace minibank.Tests
{
    public class AccountServiceTests
    {
        [Fact]
        public async Task CreateTransactionAsync_DebitExceedsBalance_ReturnsInsufficientFunds()
        {
            // Arrange
            var accounts = new List<Account>
            {
                new Account { Id = 1, Balance = 100m }
            };
            var mockAccounts = CreateMockDbSet(accounts);

            var options = new DbContextOptionsBuilder<BankingDbContext>().Options;
            var mockContext = new Mock<BankingDbContext>(options);
            mockContext.Setup(c => c.Accounts).Returns(mockAccounts.Object);

            var service = new AccountService(mockContext.Object);
            var dto = new PostTransactionDto
            {
                AccountId = 1,
                Amount = 150m,
                TransactionType = TransactionType.Debit,
                Description = "ATM withdrawal"
            };

            // Act
            var response = await service.CreateTransactionAsync(dto);

            // Assert
            Assert.False(response.Success);
            Assert.Equal("Insufficient funds.", response.Message);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateTransactionAsync_ClosedAccount_ReturnsFailure()
        {
            // Arrange
            var accounts = new List<Account>
            {
                new Account { Id = 1, Balance = 100m, IsActive = false }
            };
            var mockAccounts = CreateMockDbSet(accounts);

            var options = new DbContextOptionsBuilder<BankingDbContext>().Options;
            var mockContext = new Mock<BankingDbContext>(options);
            mockContext.Setup(c => c.Accounts).Returns(mockAccounts.Object);

            var service = new AccountService(mockContext.Object);
            var dto = new PostTransactionDto
            {
                AccountId = 1,
                Amount = 50m,
                TransactionType = TransactionType.Credit,
                Description = "Teller deposit"
            };

            // Act
            var response = await service.CreateTransactionAsync(dto);

            // Assert
            Assert.False(response.Success);
            Assert.Equal("Closed accounts cannot accept transactions.", response.Message);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression)
                .Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType)
                .Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator())
                .Returns(() => queryable.GetEnumerator());

            return mockSet;
        }
    }
}
