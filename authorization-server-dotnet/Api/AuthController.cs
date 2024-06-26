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
    private IConfiguration _configuration;
    private ITokenService _tokenService;

    public AuthController(IApplicationDbContext dbContext, IConfiguration configuration, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenData>> Login([FromBody] LoginCommand command){
        var loginCommandHandler = new LoginCommandHandler(_dbContext, _configuration, _tokenService);
        return (await loginCommandHandler.Handle(command)).Match<ActionResult>(
            success => new OkObjectResult(success),
            exception => new BadRequestObjectResult(exception.Message)
        );
    }
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<bool>> Logout([FromBody] string refreshToken){
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
}