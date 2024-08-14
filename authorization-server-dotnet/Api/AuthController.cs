using AuthService.Application.Commands;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;
    private readonly ILogger _logger;

    public AuthController(IApplicationDbContext dbContext, IConfiguration configuration, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenData>> Login([FromBody] LoginCommand command){
        LogAction("login", command.AccountName);
        
        var loginCommandHandler = new LoginCommandHandler(_dbContext, _configuration, _tokenService);
        return (await loginCommandHandler.Handle(command)).Match<ActionResult>(
            success => new OkObjectResult(success),
            exception => new BadRequestObjectResult(exception.Message)
        );
    }
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<bool>> Logout([FromBody] string refreshToken){
        LogAction("logout", "---");

        var logoutCommandHandler = new LogoutCommandHandler(_dbContext, _tokenService);
        var command = new LogoutCommand() {
            AccountId = Convert.ToInt32(GetClaimFromToken(HttpContext.User, _configuration["Auth:UserClaim"])),
            RefreshToken = refreshToken
        };

        return (await logoutCommandHandler.Handle(command)).Match<ActionResult>(
            success => new OkObjectResult(success),
            exception => new BadRequestObjectResult(exception.Message)
        );
    }

    [Authorize]
    [HttpPost("refresh")]
    public async Task<ActionResult<TokenData>> Refresh([FromBody] string refreshToken){
        LogAction("refresh", "---");
                
        var refreshTokenCommandHandler = new RefreshTokenCommandHandler(_dbContext, _configuration, _tokenService);
        var command = new RefreshTokenCommand() {
            AccountId = Convert.ToInt32(GetClaimFromToken(HttpContext.User, _configuration["Auth:UserClaim"])),
            RefreshToken = refreshToken
        };

        return (await refreshTokenCommandHandler.Handle(command)).Match<ActionResult>(
            success => new OkObjectResult(success),
            exception => new BadRequestObjectResult(exception.Message)
        );
    }

    private string GetClaimFromToken(ClaimsPrincipal claimsPrincipal, string claimType){
        var claim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == claimType);

        if(claim != null){
            return claim.Value;
        }

        return string.Empty;
    }

    private void LogAction(string actionName, string actionParameter){
        var caller = HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR") ?? HttpContext.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation($"-- Action: {actionName}, action param: {actionParameter}. Call from: {caller} --");
    }
}