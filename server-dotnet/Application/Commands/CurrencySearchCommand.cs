using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Models;

namespace TransactionService.Application.Commands;

public class CurrencySearchCommand {
    public string Code { get; set; }
    public int? Id { get; set; }
    public string Description { get; set; }
    public bool? Active { get; set; }
}

public class CurrencySearchCommandHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _currencyMapper;

    public CurrencySearchCommandHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _currencyMapper = mapper;
    }

    public async Task<List<CurrencyDto>> Handle(CurrencySearchCommand command)
    {
        var currencyQuery = _dbContext.Currencies.Where(x => true);

        if(!string.IsNullOrEmpty(command.Code)){
            var code = command.Code.ToLower();
            currencyQuery = currencyQuery.Where(x => x.Code.ToLower().Contains(code));
        }

        if(command.Id.HasValue)
            currencyQuery = currencyQuery.Where(x => x.Id == command.Id);

        if(command.Active.HasValue)
            currencyQuery = currencyQuery.Where(x => x.Active == command.Active);

        if(!string.IsNullOrEmpty(command.Description)){
            var description = command.Description.ToLower();
            currencyQuery = currencyQuery.Where(x => x.Description.ToLower().Contains(description));
        }

        return await currencyQuery.Select(x => _currencyMapper.Map<CurrencyDto>(x)).ToListAsync();
    }
}