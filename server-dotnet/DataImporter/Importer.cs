using System.Globalization;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Types;

namespace TransactionService.DataImporter;

public class TransactionRow{
    [CsvHelper.Configuration.Attributes.Index(0)]
    public DateTime Date { get; set; }

    [CsvHelper.Configuration.Attributes.Index(1)]
    public int AccountId { get; set; }

    [CsvHelper.Configuration.Attributes.Index(2)]
    public string AccountName { get; set; }

    [CsvHelper.Configuration.Attributes.Index(3)]
    public double Amount { get; set; }

    [CsvHelper.Configuration.Attributes.Index(4)]
    public string Payee { get; set; }

    [CsvHelper.Configuration.Attributes.Index(5)]
    public string CategoryId { get; set; }

    [CsvHelper.Configuration.Attributes.Index(6)]
    public string CategoryName { get; set; }

    [CsvHelper.Configuration.Attributes.Index(7)]
    public string SubCategoryId { get; set; }

    [CsvHelper.Configuration.Attributes.Index(8)]
    public string SubCategoryName { get; set; }

    [CsvHelper.Configuration.Attributes.Index(9)]
    public string Comment { get; set; }
}

public class Importer {
    private IApplicationDbContext _dbContext;

    public Importer(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ImportFromCsv(string file){
        IEnumerable<TransactionRow> records = new List<TransactionRow>();

        file = "DataImporter/"+file+".csv";

        using (var reader = new StreamReader(file))
        using (var csv = new CsvReader(reader, CultureInfo.GetCultureInfo("en-GB")))
        {
            try{
                records = csv.GetRecords<TransactionRow>().ToList();
            } catch(Exception ex) { 
                var x = ex;

            }
        }

        var accounts = await _dbContext.Accounts.ToDictionaryAsync(x => x.Id, x => x );
        var categories = await _dbContext.Categories.ToDictionaryAsync(x => x.Id, x => x);

        foreach(TransactionRow record in records){
            if (record.AccountId <= 0)
                throw new Exception("no account");

            if (!accounts.ContainsKey(record.AccountId)){
                throw new Exception("no account");
            }

            if (string.IsNullOrEmpty(record.CategoryId))
                throw new Exception("no category");
            
            if (string.IsNullOrEmpty(record.SubCategoryId))
                throw new Exception("no subcategory");

            string newCategoryId = record.CategoryId + record.SubCategoryId;
            int newCategoryIdInt = -1;
            if (newCategoryId == "11-") { newCategoryIdInt = 1001; }
            else if (newCategoryId == "10-") { newCategoryIdInt = 1000; }
            else { newCategoryIdInt = Convert.ToInt32(newCategoryId); }

            if(!categories.ContainsKey(newCategoryIdInt)){
                throw new Exception("sth wen wrong with category");
            }


            await _dbContext.Transactions.AddAsync(new Transaction(){
                OwnerId = 1,
                Date = record.Date.ToUniversalTime(),
                Account = accounts[record.AccountId],
                Amount = record.Amount,
                Category = categories[newCategoryIdInt],
                Comment = record.Payee,
                Active = true
            });
        }

        var xx = 10;
        await _dbContext.SaveChangesAsync();
    }
}