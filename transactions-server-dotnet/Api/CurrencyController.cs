using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.Models;
using TransactionService.Application.Commands;
using TransactionService.Application.Interfaces;
using AutoMapper;

namespace TransactionService.Api;

[ApiController]
[Route("api/[controller]")]
public class CurrencyController
{
    private IApplicationDbContext _dbContext;
    private IMapper _mapper;

    public CurrencyController(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext; 
        _mapper = mapper;
    }

    [HttpPost("search")]
    public async Task<ActionResult<List<CurrencyDto>>> Search([FromBody] CurrencySearchCommand command){
        var currencySearchCommandHandler = new CurrencySearchCommandHandler(_dbContext, _mapper);
        return await currencySearchCommandHandler.Handle(command);
    }
}