namespace minibank.DTOs
{
    public class TransactionDto
    {
        public int Id{get;set;}
        public decimal Amount{get;set;}
        public string Type{get;set;}=string.Empty;
        public string Description{get;set;} = string.Empty;
        public DateTime TransactionDate{get;set;}
        
    }
}