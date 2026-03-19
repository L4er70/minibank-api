using System.ComponentModel.DataAnnotations;
namespace minibank.DTOs
{
    public class TransferDto
    {
        [Required]
        public int FromAccountId{get;set;}

        [Required]
        public int ToAccountId{get;set;}

        [Required]
        [Range(0.01 , double.MaxValue,ErrorMessage ="Transfer amount must be grater than zero.")]
        public decimal Amount {get;set;}
    }

}