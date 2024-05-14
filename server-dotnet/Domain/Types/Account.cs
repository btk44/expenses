namespace TransactionService.Domain.Types;

public class Account: Base {
    public int OwnerId { get; set; }
    public string Name { get; set; }
    public int CurrencyId { get; set; }
    public double Amount => AccountTransactionSum != null ? AccountTransactionSum.Amount : 0;
        
    // navigation props
    public Currency Currency { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
    public AccountTransactionSum AccountTransactionSum { get; set; }
}