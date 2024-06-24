using AuthService.Domain.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AuthService.Application.Interfaces;

public interface IApplicationDbContext{
    public DbSet<Account> Accounts { get; } 
    public DbSet<RefreshToken> RefreshTokens { get; } 
    public DbSet<LoginAttempt> LoginAttempts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    ChangeTracker ChangeTracker { get; }
}