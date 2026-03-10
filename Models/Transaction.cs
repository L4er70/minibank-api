using System.ComponentModel.DataAnnotations;
using minibank.Enums;

namespace minibank.Models
{
    public class Transaction
    {
        public int Id{get;set;}

        public decimal Amount{get;set;}
        public TransactionType Type{get;set;}

        [MaxLength(200)]
        public string Description{get;set;} = string.Empty;

        public DateTime TransactionDate{get;set;}= DateTime.UtcNow;

        //foreign key
        public int AccountId{get;set;}
        public Account Account{get;set;}=null!;
    }
}