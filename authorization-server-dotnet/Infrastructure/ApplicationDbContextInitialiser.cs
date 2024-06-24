using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Types;

namespace AuthService.Infrastructure;

public class ApplicationDbContextInitialiser{
    private ApplicationDbContext _dbContext;

    public ApplicationDbContextInitialiser(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Migrate(){
        await _dbContext.Database.MigrateAsync();
        await InsertData();  // to do: remove it later?
    }

    private async Task InsertData(){
        if(!_dbContext.Accounts.Any()){
            Console.WriteLine("=== Inserting account data ===");

            var accounts = new List<Account>(){
                new() { Name = "Test", Password = "test", Active = true },
            };

            _dbContext.Accounts.AddRange(accounts);         
            await _dbContext.SaveChangesAsync();
        }
    }
}