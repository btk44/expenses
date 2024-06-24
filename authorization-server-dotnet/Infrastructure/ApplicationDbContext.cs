using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AuthService.Application.Interfaces;
using AuthService.Domain.Types;

namespace AuthService.Infrastructure;
public class ApplicationDbContext : DbContext, IApplicationDbContext{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options){ }

    public DbSet<Account> Accounts { get; set;} 
    public DbSet<RefreshToken> RefreshTokens { get; set;} 
    public DbSet<LoginAttempt> LoginAttempts { get; set;} 

    protected override void OnModelCreating(ModelBuilder modelBuilder) { 
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>().ToTable("Account"); 
        modelBuilder.Entity<RefreshToken>().ToTable("RefreshToken"); 
        modelBuilder.Entity<LoginAttempt>().ToTable("LoginAttempt"); 

        modelBuilder.Entity<Account>().HasMany(x => x.RefreshTokens).WithOne(x => x.Account);        
        modelBuilder.Entity<Account>().HasOne(x => x.LoginAttempt).WithOne(x => x.Account).HasForeignKey<LoginAttempt>(x => x.AccountId);
        modelBuilder.Entity<LoginAttempt>().HasKey(x => x.AccountId);
        modelBuilder.Entity<Account>().Property(e => e.Active).IsRequired().HasDefaultValue(true);
    }    
}