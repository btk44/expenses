using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Common;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Types;

namespace TransactionService.Application.Commands;

public class AccountAmountRecalculateCommand {
    public int OwnerId { get; set; }
    public int[] Accounts { get; set; }
}

public class AccountAmountRecalculateCommandHandler
{
    private IApplicationDbContext _dbContext;
    
    public AccountAmountRecalculateCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Either<bool, AppException>> Handle(AccountAmountRecalculateCommand command)
    {
        var calculatedAmounts = await _dbContext.Transactions
                                        .Where(x => x.OwnerId == command.OwnerId && command.Accounts.Contains(x.AccountId))
                                        .GroupBy(x => x.AccountId)
                                        .Select(g => new { AccountId = g.Key, Amount = g.Sum(s => s.Amount)})
                                        .ToListAsync();

        var accountTransactionSums = await _dbContext.AccountTransactionSums
                                        .Where(x => x.OwnerId == command.OwnerId && command.Accounts.Contains(x.AccountId)).ToListAsync();

        foreach(var calculatedAmount in calculatedAmounts){
            var accountTransactionSum = accountTransactionSums.FirstOrDefault(x => x.AccountId == calculatedAmount.AccountId);

            if(accountTransactionSum == null){
                accountTransactionSum = new AccountTransactionSum() { 
                    AccountId = calculatedAmount.AccountId,
                    OwnerId = command.OwnerId
                };

                _dbContext.AccountTransactionSums.Add(accountTransactionSum);
            } 

            accountTransactionSum.Amount = calculatedAmount.Amount;
        }

        if(_dbContext.ChangeTracker.HasChanges() && await _dbContext.SaveChangesAsync() <= 0){
            return new AppException("Save error - please try again");
        }

        return true;
    }
}