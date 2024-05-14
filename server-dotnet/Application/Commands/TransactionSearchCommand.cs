using TransactionService.Domain.Types;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Models;

namespace TransactionService.Application.Commands;

public class TransactionSearchCommand {
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? Id { get; set; }
    public int OwnerId { get; set; }
    public double? AmountFrom { get; set; } 
    public double? AmountTo { get; set; }
    public List<int> Categories { get; set; }
    public string Comment { get; set; }
    public bool? Active { get; set; }
    public List<int> Accounts { get; set; }
    public int? Take { get; set; }
    public int? Offset { get; set; }
}

public class TransactionSearchCommandHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _transactionMapper;

    public TransactionSearchCommandHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _transactionMapper = mapper;
    }

    public async Task<List<TransactionDto>> Handle(TransactionSearchCommand command)
    {
        return await BuildTransactionSearchQuery(command).Select(x => _transactionMapper.Map<TransactionDto>(x)).ToListAsync();
    }

    public async Task<int> HandleCount(TransactionSearchCommand command)
    {
        return await BuildTransactionSearchQuery(command).CountAsync();
    }

    public async Task<double> HandleSum(TransactionSearchCommand command)
    {
        return await BuildTransactionSearchQuery(command).SumAsync(x => x.Amount);
    }

    private IQueryable<Transaction> BuildTransactionSearchQuery(TransactionSearchCommand command){
        var transactionQuery = _dbContext.Transactions.Where(x => x.OwnerId == command.OwnerId);
        
        if (command.AmountFrom.HasValue)
            transactionQuery = _dbContext.Transactions.Where(x => x.Amount >= command.AmountFrom);

        if (command.AmountTo.HasValue)
            transactionQuery = _dbContext.Transactions.Where(x => x.Amount <= command.AmountTo);

        if (command.DateFrom.HasValue)
            transactionQuery = _dbContext.Transactions.Where(x => x.Date  >= command.DateFrom);

        if (command.DateTo.HasValue)
            transactionQuery = _dbContext.Transactions.Where(x => x.Date <= command.DateTo);

        if( command.Id.HasValue)
            transactionQuery = transactionQuery.Where(x => x.Id == command.Id);

        if(command.Active.HasValue)
            transactionQuery = transactionQuery.Where(x => x.Active == command.Active);

        if(!string.IsNullOrEmpty(command.Comment)){
            var comment = command.Comment.ToLower();
            transactionQuery = transactionQuery.Where(x => x.Comment.ToLower().Contains(comment));
        }

        if(command.Categories != null && command.Categories.Any())
            transactionQuery = transactionQuery.Where(x => command.Categories.Contains(x.CategoryId));

        if(command.Accounts != null && command.Accounts.Any())
            transactionQuery = transactionQuery.Where(x => command.Accounts.Contains(x.AccountId));

        transactionQuery = transactionQuery.OrderBy(x=> x.Date);

        if(command.Offset.HasValue)
            transactionQuery = transactionQuery.Skip((int)command.Offset);

        if(command.Take.HasValue)
            transactionQuery = transactionQuery.Take((int)command.Take);

        return transactionQuery;
    }
}