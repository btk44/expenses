using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Types;

namespace TransactionService.Infrastructure;

public class ApplicationDbContextInitialiser{
    private ApplicationDbContext _dbContext;

    public ApplicationDbContextInitialiser(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Migrate(){
        await _dbContext.Database.MigrateAsync();
        //await InsertData();  // to do: remove it later?
    }

    private async Task InsertData(){
        if(!_dbContext.Currencies.Any()){
            Console.WriteLine("=== Inserting currency data ===");

            var currencies = new List<Currency>() {
                new Currency() { Description = "United States dollar", Code = "USD" },
                new Currency() { Description = "Euro", Code = "EUR" },
                new Currency() { Description = "Japanese yen", Code = "JPY", Active = false},
                new Currency() { Description = "Sterling", Code = "GBP" },
                new Currency() { Description = "Australian dollar", Code = "AUD", Active = false},
                new Currency() { Description = "Canadian dollar", Code = "CAD", Active = false},
                new Currency() { Description = "Swiss franc", Code = "CHF" },
                new Currency() { Description = "Renminbi", Code = "CNY", Active = false},
                new Currency() { Description = "Hong Kong dollar", Code = "HKD", Active = false},
                new Currency() { Description = "New Zealand dollar", Code = "NZD", Active = false},
                new Currency() { Description = "Swedish krona", Code = "SEK", Active = false},
                new Currency() { Description = "South Korean won", Code = "KRW", Active = false},
                new Currency() { Description = "Singapore dollar", Code = "SGD", Active = false},
                new Currency() { Description = "Norwegian krone", Code = "NOK", Active = false},
                new Currency() { Description = "Mexican peso", Code = "MXN", Active = false},
                new Currency() { Description = "Indian rupee", Code = "INR", Active = false},
                new Currency() { Description = "Russian ruble", Code = "RUB", Active = false},
                new Currency() { Description = "South African rand", Code = "ZAR", Active = false},
                new Currency() { Description = "Turkish lira", Code = "TRY", Active = false},
                new Currency() { Description = "Brazilian real", Code = "BRL", Active = false},
                new Currency() { Description = "New Taiwan dollar", Code = "TWD", Active = false},
                new Currency() { Description = "Danish krone", Code = "DKK", Active = false},
                new Currency() { Description = "Polish złoty", Code = "PLN" },
                new Currency() { Description = "Thai baht", Code = "THB", Active = false},
                new Currency() { Description = "Indonesian rupiah", Code = "IDR", Active = false},
                new Currency() { Description = "Hungarian forint", Code = "HUF", Active = false},
                new Currency() { Description = "Czech koruna", Code = "CZK", Active = false},
                new Currency() { Description = "Israeli new shekel", Code = "ILS", Active = false},
                new Currency() { Description = "Chilean peso", Code = "CLP", Active = false},
                new Currency() { Description = "Philippine peso", Code = "PHP", Active = false},
                new Currency() { Description = "UAE dirham", Code = "AED", Active = false},
                new Currency() { Description = "Colombian peso", Code = "COP", Active = false},
                new Currency() { Description = "Saudi riyal", Code = "SAR", Active = false},
                new Currency() { Description = "Malaysian ringgit", Code = "MYR", Active = false},
                new Currency() { Description = "Romanian leu", Code = "RON", Active = false}
            };

            _dbContext.Currencies.AddRange(currencies);         

            await _dbContext.SaveChangesAsync();

            var plnId = currencies.First(x => x.Code == "PLN").Id;
            var eurId = currencies.First(x => x.Code == "EUR").Id;
            var accounts = new List<Account>(){
                new Account() { Name = "mBank", AccountTransactionSum = new AccountTransactionSum(){ Amount = 0 }, CurrencyId = plnId, OwnerId = 1},
                new Account() { Name = "Millenium", AccountTransactionSum = new AccountTransactionSum(){ Amount = 0 }, CurrencyId = plnId, OwnerId = 1},
                new Account() { Name = "Millenium Profit", AccountTransactionSum = new AccountTransactionSum(){ Amount = 0 }, CurrencyId = plnId, OwnerId = 1},
                new Account() { Name = "BGZ Optima", AccountTransactionSum = new AccountTransactionSum(){ Amount = 0 }, CurrencyId = plnId, OwnerId = 1},
                new Account() { Name = "Alior", AccountTransactionSum = new AccountTransactionSum(){ Amount = 0 }, CurrencyId = plnId, Active = false, OwnerId = 1},
                new Account() { Name = "Aion", AccountTransactionSum = new AccountTransactionSum(){ Amount = 0 }, CurrencyId = eurId, OwnerId = 1},
                new Account() { Name = "Millenium Marta", AccountTransactionSum = new AccountTransactionSum(){ Amount = 0 }, CurrencyId = plnId, OwnerId = 1}
            };

            _dbContext.Accounts.AddRange(accounts);         
            await _dbContext.SaveChangesAsync();

            var categories = new List<Category>() {
                new Category() { Name = "food", OwnerId = 1 },
                new Category() { Name = "some stuff", OwnerId = 1 },
                new Category() { Name = "kids", OwnerId = 1 },
                new Category() { Name = "transfer", OwnerId = 1 }
            };

            _dbContext.Categories.AddRange(categories);
            await _dbContext.SaveChangesAsync();

            var foodId = categories.First(x => x.Name == "food").Id;
            var subCategories = new List<Category>(){
                new Category() { Name = "restaurant", OwnerId = 1, ParentId = foodId },
                new Category() { Name = "groceries", OwnerId = 1, ParentId = foodId }
            };

            _dbContext.Categories.AddRange(subCategories);
            await _dbContext.SaveChangesAsync();
        }
    }
}