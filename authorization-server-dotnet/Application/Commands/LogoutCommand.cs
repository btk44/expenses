using AuthService.Application.Common;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Commands;

public class LogoutCommand{
    public string AccountName { get; set; }
    public string RefreshToken { get; set; }
}

public class LogoutCommandHandler {
    private IApplicationDbContext _dbContext;
    private ITokenService _tokenService;

    public LogoutCommandHandler(IApplicationDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;   
        _tokenService = tokenService;
    }

    public async Task<Either<bool, AppException>> Handle(LogoutCommand command){
        var account = await _dbContext.Accounts
                                        .Include(x => x.RefreshTokens)
                                        .Include(x => x.LoginAttempt)
                                        .FirstOrDefaultAsync(x => x.Active && x.Name == command.AccountName);

        account.RefreshTokens.Remove(account.RefreshTokens.FirstOrDefault(rf => rf.Token == command.RefreshToken));
        await _dbContext.SaveChangesAsync();

        return true;
    }
}


