using System.ComponentModel.DataAnnotations;
namespace minibank.DTOs
{
    public class CrossCustomerTransferDto
    {
        [Required]
        public int FromAccountId{get;set;}
        [Required]
        [StringLength(20,MinimumLength =8,ErrorMessage ="Invalid Account Number Format")]
        public string ToAccounNumber{get;set;}

        [Required]
        [Range(0.01,double.MaxValue,ErrorMessage ="Amount must be greater than zero.")]
        public decimal Amount{get;set;}
    }
}