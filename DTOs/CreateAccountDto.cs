using minibank.Enums;

namespace minibank.DTOs
{
    public class CreateAccountDto
    {
        public int CustomerId{get;set;}
        public Currency currency{get;set;}
        public AccountType accountType{get;set;}
        public string BranchCode{get;set;} = "BKT01";
    }
}