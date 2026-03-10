using System.ComponentModel.DataAnnotations;
using minibank.Enums;
namespace minibank.Models
{
    public class Customer
    {
        public int Id{get;set;}

        [Required]
        [MaxLength(20)]
        public string PersonalId{get;set;} = string.Empty; //NID

        [Required]
        [MaxLength(50)]
        public string FirstName{get;set;} = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName{get;set;} = string.Empty;

        public DateTime DateOfBirth{get;set;}
        public DateTime CreatedAt{get;set;} = DateTime.UtcNow;

        //one customer has many accounts (1:N)
        public List<Account> Accounts{get;set;} = new List<Account>();
    }
}