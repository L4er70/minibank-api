using System.ComponentModel.DataAnnotations;
namespace minibank.DTOs
{
    public class CreateCustomerDto
    {
        [Required(ErrorMessage ="NID is required for banking registration")]
        [StringLength(20)]
        public string PersonalId{get;set;} =string.Empty;

        [Required]
        public string FirstName{get;set;} = string.Empty;

        [Required]
        public string LastName{get;set;} = string.Empty;
        
        [Required]
        public DateTime DateOfBirth{get;set;}
    }
}