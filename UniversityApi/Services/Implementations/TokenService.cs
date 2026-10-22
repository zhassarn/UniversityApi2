using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using UniversityApi.Common;
using UniversityApi.Models;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options) => _options = options.Value;

    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(AppClaims.Sub, user.Id),
            new(AppClaims.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(AppClaims.Role, r)));

        // Студент/оқытушы аккаунтының иелігін тексеру үшін қажет claim-дер
        if (user.StudentId.HasValue) claims.Add(new Claim(AppClaims.StudentId, user.StudentId.Value.ToString()));
        if (user.TeacherId.HasValue) claims.Add(new Claim(AppClaims.TeacherId, user.TeacherId.Value.ToString()));

        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            NotBefore = DateTime.UtcNow,
            Expires = expires,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expires);
    }
}
