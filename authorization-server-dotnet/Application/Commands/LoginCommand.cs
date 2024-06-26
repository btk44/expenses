using AuthService.Application.Common;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Types;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Commands;

public class LoginCommand{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class LoginCommandHandler {
    private IApplicationDbContext _dbContext;
    private IPasswordHasher<string> _passwordHasher;
    private IConfiguration _configuration;
    private ITokenService _tokenService;

    public LoginCommandHandler(IApplicationDbContext dbContext, IConfiguration configuration, ITokenService tokenService)
    {
        _dbContext = dbContext;  
        _passwordHasher = new PasswordHasher<string>(); 
        _configuration = configuration;
        _tokenService = tokenService;
    }

    public async Task<Either<TokenData, AppException>> Handle(LoginCommand command){
        if(string.IsNullOrEmpty(command.AccountName) || string.IsNullOrEmpty(command.Password))
            return new AppException("Missing login data");

        var account = await _dbContext.Accounts
                                        .Include(x => x.RefreshTokens)
                                        .Include(x => x.LoginAttempt)
                                        .FirstOrDefaultAsync(x => x.Active && x.Name == command.AccountName);

        if(account == null)
            return new AppException("Account does not exist");

        var maxLoginAttempts = Convert.ToInt32(_configuration["Auth:MaxFailedLogins"]);
        var blockAccountTime = TimeSpan.FromMinutes(Convert.ToInt32(_configuration["Auth:BlockAccountTimeInMinutes"]));

        if (account.LoginAttempt == null)
            account.LoginAttempt = new LoginAttempt(){ FailedAttemptsCount = 0, LastAttemptDate = DateTime.Now };

        var timeFromLastAttempt = DateTime.Now - account.LoginAttempt.LastAttemptDate;
        if (account.LoginAttempt.FailedAttemptsCount > maxLoginAttempts)
            if (timeFromLastAttempt < blockAccountTime)
                return new AppException("Account is blocked");
            else
                account.LoginAttempt.FailedAttemptsCount = 0;      
        
        account.LoginAttempt.LastAttemptDate = DateTime.Now;

        if (_passwordHasher.VerifyHashedPassword(account.Name, account.Password, command.Password) != PasswordVerificationResult.Success){
            account.LoginAttempt.FailedAttemptsCount++;
            await _dbContext.SaveChangesAsync();
            return new AppException("Incorrect password");
        }
        else {
            account.LoginAttempt.FailedAttemptsCount = 0;
        }

        var now = DateTime.Now;
        var tokenData = _tokenService.CreateTokenData(new Dictionary<string, string>() { {_configuration["Auth:UserClaim"], account.Id.ToString()} });

        if (tokenData == null)
            return new AppException("Token generation error");

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