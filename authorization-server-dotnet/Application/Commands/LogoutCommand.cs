using AuthService.Application.Common;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Commands;

public class LogoutCommand{
    public int AccountId { get; set; }
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
                                        .FirstOrDefaultAsync(x => x.Active && x.Id == command.AccountId);

        if(account == null)
            return new AppException("Account does not exist");

        account.RefreshTokens.Remove(account.RefreshTokens.FirstOrDefault(rf => rf.Token == command.RefreshToken));
        await _dbContext.SaveChangesAsync();

        return true;
    }
}


