using System.Security.Claims;

namespace AuthService.Application.Services;

public interface ITokenService
{
    TokenData CreateTokenData(Dictionary<string, string> tokenClaims);
    string GenerateRefreshToken();
    string GetClaimFromToken(ClaimsPrincipal claimPrincipal, string claimName);
}