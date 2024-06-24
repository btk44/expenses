using AuthService.Application.Common;
using AuthService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Commands;

public class RefreshTokenCommand{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class RefreshTokenCommandHandler {
    private IApplicationDbContext _dbContext;

    public RefreshTokenCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;   
    }

    public async Task<Either<bool, AppException>> Handle(RefreshTokenCommand command){
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Active && x.Name == command.AccountName);

        return true;
    }
}