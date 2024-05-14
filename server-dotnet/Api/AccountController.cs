using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.Models;
using TransactionService.Application.Commands;
using AutoMapper;
using TransactionService.Application.Interfaces;
//using TransactionService.DataImporter;

namespace TransactionService.Api;

[ApiController]
[Route("api/[controller]")]
public class AccountController
{
    private IApplicationDbContext _dbContext;
    private IMapper _mapper;

    public AccountController(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpPost("search")]
    public async Task<ActionResult<List<AccountDto>>> Search([FromBody] AccountSearchCommand command){
        var accountSearchCommandHandler = new AccountSearchCommandHandler(_dbContext, _mapper);
        return await accountSearchCommandHandler.Handle(command);
    }

    [HttpPost("calculate-amount")]
    public async Task<ActionResult<bool>> Search([FromBody] AccountAmountRecalculateCommand command){
        var accountAmountRecalculateCommandHandler = new AccountAmountRecalculateCommandHandler(_dbContext);
        return (await accountAmountRecalculateCommandHandler.Handle(command)).Match<ActionResult>(
            success => new OkObjectResult(success),
            exception => new BadRequestObjectResult(exception.Message)
        );
    }

    // [HttpGet()]
    // public async Task<ActionResult<bool>> ImportStuff(){ // to do : remove it later
    //     var importer = new Importer(_dbContext);
    //     await importer.ImportFromCsv("bd-n-all");
    //     return true;
    // }
}