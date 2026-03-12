using minibank.Enums;
namespace minibank.DTOs
{
    public class PostTransactionDto
    {
        public int AccountId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
    }
}