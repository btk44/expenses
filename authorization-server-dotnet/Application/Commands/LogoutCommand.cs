using AuthService.Application.Common;
using AuthService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Commands;

public class LogoutCommand{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class LogoutCommandHandler {
    private IApplicationDbContext _dbContext;

    public LogoutCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;   
    }

    public async Task<Either<bool, AppException>> Handle(LogoutCommand command){
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Active && x.Name == command.AccountName);

        return true;
    }
}