using System.Security.Claims;
using System.Text;
using IdleMmo.Application.Common.Abstractions;
using IdleMmo.Application.Common.Time;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IdleMmo.Infrastructure.Auth;

public sealed class JwtTokenIssuer : IJwtTokenIssuer
{
    private readonly AuthOptions _options;
    private readonly IClock _clock;
    private readonly SigningCredentials _credentials;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtTokenIssuer(IOptions<AuthOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;

        if (string.IsNullOrWhiteSpace(_options.JwtSigningKey))
        {
            throw new InvalidOperationException("Auth.JwtSigningKey must be configured.");
        }
        var keyBytes = Encoding.UTF8.GetBytes(_options.JwtSigningKey);
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("Auth.JwtSigningKey must be at least 32 bytes for HS256.");
        }
        _credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
    }

    public AccessTokenIssuance IssueAccessToken(long accountId, string displayName, bool isGuest)
    {
        var now = _clock.UtcNow;
        var expires = now.Add(_options.AccessTokenLifetime);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.JwtIssuer,
            Audience = _options.JwtAudience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = _credentials,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),
                new Claim("display_name", displayName),
                new Claim("guest", isGuest ? "1" : "0"),
            }),
        };
        string token = _handler.CreateToken(descriptor);
        return new AccessTokenIssuance(token, expires);
    }
}
