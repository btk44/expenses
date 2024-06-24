using AuthService.Application.Common;
using AuthService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Commands;

public class LoginCommand{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class LoginCommandHandler {
    private IApplicationDbContext _dbContext;

    public LoginCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;   
    }

    public async Task<Either<bool, AppException>> Handle(LoginCommand command){
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Active && x.Name == command.AccountName);

        return true;
    }
}