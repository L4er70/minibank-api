using System.ComponentModel.DataAnnotations;
using System.Transactions;
using minibank.Enums;
namespace minibank.Models
{
    public class Account
    {
        public int Id{get;set;}
        [Required]
        [MaxLength(30)]
        public string AccountNumber{get;set;} = string.Empty; //IBAN

        public decimal Balance{get;set;} =0;
        public Currency Currency{get;set;}
        public AccountType AccountType{get;set;}

        [MaxLength(10)]
        public string BranchCode{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}= DateTime.UtcNow;

        //foreign key
        public int CustomerId{get;set;}

        public Customer Customer{get;set;} = null!; //the owner of the account
        public List<Transaction> Transactions{get;set;}= new List<Transaction>();//1:N
    }
}