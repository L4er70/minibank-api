namespace minibank.DTOs
{
    public class AccountDto
{
    public int Id{get;set;}
    public string AccountNumber{get;set;} = string.Empty;
    public decimal Balance {get;set;}
    public string Currency {get;set;}=string.Empty;
    public string AccountType{get;set;} = string.Empty;
    public string BranchCode{get;set;} = string.Empty;
    public DateTime CreatedAt{get;set;}
    public bool IsActive { get; set; }
}
}
