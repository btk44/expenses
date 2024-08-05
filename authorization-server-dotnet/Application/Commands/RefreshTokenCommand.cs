using AuthService.Application.Common;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Types;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Commands;

public class RefreshTokenCommand{
    public int AccountId { get; set; }
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
        var account = await _dbContext.Accounts
                                        .Include(x => x.RefreshTokens)
                                        .FirstOrDefaultAsync(x => x.Active && x.Id == command.AccountId);

        if(account == null)
            return new AppException("Account does not exist");

        var utcNow = DateTime.UtcNow; // see TokenService
        if(!account.RefreshTokens.Any(x => x.Token == command.RefreshToken && x.ExpiresAtUtc > utcNow)){
            return new AppException("Incorrect or expired refresh token");
        }

        var tokenData = _tokenService.CreateTokenData(new Dictionary<string, string>() { {_configuration["Auth:UserClaim"], account.Id.ToString()} });

        if (tokenData == null)
            return new AppException("Token generation error");

        var refreshToken = new RefreshToken() { Token = tokenData.RefreshToken, 
                                                ExpiresAtUtc = tokenData.RefreshExpirationTime,
                                                AccountId = account.Id,
                                                Account = account };

        account.RefreshTokens.Add(refreshToken);

        _dbContext.RefreshTokens.RemoveRange(_dbContext.RefreshTokens.Where(x => x.AccountId == account.Id && 
                                                                                 (x.ExpiresAtUtc < utcNow || x.Token == command.RefreshToken)));
        await _dbContext.SaveChangesAsync();

        return tokenData;
    }
}