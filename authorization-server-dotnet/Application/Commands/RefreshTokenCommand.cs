using AuthService.Application.Common;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Types;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Commands;

public class RefreshTokenCommand{
    public string AccountName { get; set; }
    public string RefreshToken { get; set; }
}

public class RefreshTokenCommandHandler {
    private IApplicationDbContext _dbContext;
    private IConfiguration _configuration;
    private ITokenService _tokenService;

    public RefreshTokenCommandHandler(IApplicationDbContext dbContext, IConfiguration configuration, ITokenService tokenService)
    {
        _dbContext = dbContext;  
        _configuration = configuration;
        _tokenService = tokenService;
    }

    public async Task<Either<TokenData, AppException>> Handle(RefreshTokenCommand command){
        if(string.IsNullOrEmpty(command.AccountName) || string.IsNullOrEmpty(command.RefreshToken))
            throw new AppException("Missing login data");

        var account = await _dbContext.Accounts
                                        .Include(x => x.RefreshTokens)
                                        .Include(x => x.LoginAttempt)
                                        .FirstOrDefaultAsync(x => x.Active && x.Name == command.AccountName);

        var maxLoginAttempts = Convert.ToInt32(_configuration["Auth:MaxFailedLogins"]);
        var blockAccountTime = TimeSpan.FromMinutes(Convert.ToInt32(_configuration["Auth:BlockAccountTimeInMinutes"]));
        var timeFromLastAttempt = DateTime.Now - account.LoginAttempt.LastAttemptDate;
        
        if (account.LoginAttempt.FailedAttemptsCount > maxLoginAttempts && timeFromLastAttempt < blockAccountTime)
            throw new AppException("Account is blocked");


        var now = DateTime.Now;
        if(!account.RefreshTokens.Any(x => x.Token == command.RefreshToken && x.ExpiresAt > now)){
            throw new AppException("Incorrect or expired refresh token");
        }

        var tokenData = _tokenService.CreateTokenData(new Dictionary<string, string>() { {_configuration["Auth:UserNameClaim"], account.Name} });

        if (tokenData == null)
            throw new AppException("Token generation error");

        var refreshToken = new RefreshToken() { Token = tokenData.RefreshToken, 
                                                ExpiresAt = tokenData.RefreshExpirationTime,
                                                AccountId = account.Id,
                                                Account = account };

        account.RefreshTokens.Add(refreshToken);

        _dbContext.RefreshTokens.RemoveRange(_dbContext.RefreshTokens.Where(x => x.AccountId == account.Id && x.ExpiresAt < now));
        await _dbContext.SaveChangesAsync();

        return tokenData;
    }
}