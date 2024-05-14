using TransactionService.Domain.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TransactionService.Application.Interfaces;

public interface IApplicationDbContext{
    public DbSet<Account> Accounts { get; } 
    public DbSet<AccountTransactionSum> AccountTransactionSums { get; } 
    public DbSet<Transaction> Transactions { get; }
    public DbSet<Category> Categories { get; }
    public DbSet<Currency> Currencies { get; }
    DbSet<VisualProperties> VisualProperties { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    ChangeTracker ChangeTracker { get; }
}