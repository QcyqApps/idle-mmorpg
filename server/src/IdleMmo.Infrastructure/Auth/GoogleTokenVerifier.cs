using Google.Apis.Auth;
using IdleMmo.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdleMmo.Infrastructure.Auth;

public sealed class GoogleTokenVerifier : IGoogleTokenVerifier
{
    private readonly AuthOptions _options;
    private readonly ILogger<GoogleTokenVerifier> _logger;

    public GoogleTokenVerifier(IOptions<AuthOptions> options, ILogger<GoogleTokenVerifier> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GoogleTokenClaims> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idToken);
        if (_options.Google.AndroidClientIds.Count == 0)
        {
            throw new InvalidOperationException("Auth.Google.AndroidClientIds must be configured.");
        }

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = _options.Google.AndroidClientIds,
        };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings).ConfigureAwait(false);
            return new GoogleTokenClaims(payload.Subject, payload.Email, payload.Name);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Google ID token validation failed");
            throw new UnauthorizedAccessException("Invalid Google ID token.", ex);
        }
    }
}
