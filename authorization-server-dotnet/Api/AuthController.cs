using AuthService.Application.Commands;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController
{
    private readonly IApplicationDbContext _dbContext;

    public AuthController(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenData>> Login([FromBody] LoginCommand command){
        var loginCommandHandler = new LoginCommandHandler(_dbContext);
        return (await loginCommandHandler.Handle(command)).Match<ActionResult>(
            success => new OkObjectResult(success),
            exception => new BadRequestObjectResult(exception.Message)
        );
    }
    
    [HttpPost("logout")]
    public async Task<ActionResult<bool>> Logout([FromBody] LogoutCommand command){
        var logoutCommandHandler = new LogoutCommandHandler(_dbContext);
        return (await logoutCommandHandler.Handle(command)).Match<ActionResult>(
            success => new OkObjectResult(success),
            exception => new BadRequestObjectResult(exception.Message)
        );
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenData>> Refresh([FromBody] RefreshTokenCommand command){
        var refreshTokenCommandHandler = new RefreshTokenCommandHandler(_dbContext);
        return (await refreshTokenCommandHandler.Handle(command)).Match<ActionResult>(
            success => new OkObjectResult(success),
            exception => new BadRequestObjectResult(exception.Message)
        );
    }
}