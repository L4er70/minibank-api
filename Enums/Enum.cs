namespace MINIBANK.Enums
{
    public enum AccountType
    {
        Current,  //llogari rrjedhese
        Savings  //llogari kursimi
    }

    public enum TransactionType
    {
        Credit,  //money coming IN
        Debit    //money going OUT
    }

    public enum Currency
    {
        ALL,
        EUR,
        USD,
        GBP
    }
}