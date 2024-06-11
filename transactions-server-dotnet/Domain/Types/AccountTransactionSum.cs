namespace TransactionService.Domain.Types;

public class AccountTransactionSum {
    public int OwnerId { get; set; }
    public int AccountId { get; set; }
    public double Amount { get; set; }
        
    // navigation props
    public Account Account { get; set; }
}