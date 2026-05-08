using IdleMmo.Application.Common.Abstractions;
using IdleMmo.Application.Common.Time;
using IdleMmo.Domain.Enums;
using IdleMmo.Shared.Contracts.Auth;
using Microsoft.Extensions.Logging;

namespace IdleMmo.Application.Features.Auth;

public sealed class RefreshHandler
{
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IAccountRepository _accounts;
    private readonly IJwtTokenIssuer _accessTokens;
    private readonly IAuditLogger _audit;
    private readonly IClock _clock;
    private readonly ILogger<RefreshHandler> _logger;

    public RefreshHandler(
        IRefreshTokenService refreshTokens,
        IAccountRepository accounts,
        IJwtTokenIssuer accessTokens,
        IAuditLogger audit,
        IClock clock,
        ILogger<RefreshHandler> logger)
    {
        _refreshTokens = refreshTokens;
        _accounts = accounts;
        _accessTokens = accessTokens;
        _audit = audit;
        _clock = clock;
        _logger = logger;
    }

    public async Task<AuthSessionResponse> HandleAsync(
        RefreshTokenRequest request,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RefreshToken);

        var rotation = await _refreshTokens.RotateAsync(request.RefreshToken, userAgent, ipAddress, cancellationToken)
            .ConfigureAwait(false);
        var account = await _accounts.FindByIdAsync(rotation.AccountId, cancellationToken).ConfigureAwait(false)
            ?? throw new RefreshTokenInvalidException("Account no longer exists.");

        var access = _accessTokens.IssueAccessToken(account.Id, account.DisplayName, account.IsGuest);

        await _audit.WriteAsync(
            AuditAction.RefreshTokenRotated,
            AuditSeverity.Info,
            new { accountId = account.Id },
            accountId: account.Id,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Refresh token rotated for account {AccountId}", account.Id);

        return new AuthSessionResponse(
            account.Id,
            access.Token,
            access.ExpiresAt,
            rotation.Token,
            rotation.ExpiresAt,
            account.DisplayName,
            account.IsGuest);
    }
}
