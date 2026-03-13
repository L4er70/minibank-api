using minibank.Models;
using Microsoft.EntityFrameworkCore;

namespace minibank.Data
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options): base(options)
        {
            
        }
        public virtual DbSet<Customer> Customers{get;set;}
        public virtual DbSet<Account>Accounts{get;set;}
        public virtual DbSet<Transaction> Transactions{get;set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //1-to-many relationship between Customer and Account
            modelBuilder.Entity<Customer>().HasMany(c=> c.Accounts)
            .WithOne(a=>a.Customer)
            .HasForeignKey(a=>a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

            //1-to-many relationship between  Account and Transaction
            modelBuilder.Entity<Account>()
            .HasMany(a=>a.Transactions)
            .WithOne(t=>t.Account)
            .HasForeignKey(t=>t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
