using IdleMmo.Application.Common.Abstractions;
using IdleMmo.Application.Common.Time;
using IdleMmo.Domain.Entities;
using IdleMmo.Domain.Enums;
using IdleMmo.Shared.Contracts.Auth;
using Microsoft.Extensions.Logging;

namespace IdleMmo.Application.Features.Auth;

public sealed class LoginGoogleHandler
{
    private readonly IGoogleTokenVerifier _googleVerifier;
    private readonly IAccountRepository _accounts;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IJwtTokenIssuer _accessTokens;
    private readonly IAuditLogger _audit;
    private readonly IClock _clock;
    private readonly ILogger<LoginGoogleHandler> _logger;

    public LoginGoogleHandler(
        IGoogleTokenVerifier googleVerifier,
        IAccountRepository accounts,
        IRefreshTokenService refreshTokens,
        IJwtTokenIssuer accessTokens,
        IAuditLogger audit,
        IClock clock,
        ILogger<LoginGoogleHandler> logger)
    {
        _googleVerifier = googleVerifier;
        _accounts = accounts;
        _refreshTokens = refreshTokens;
        _accessTokens = accessTokens;
        _audit = audit;
        _clock = clock;
        _logger = logger;
    }

    public async Task<AuthSessionResponse> HandleAsync(
        LoginGoogleRequest request,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.IdToken);

        var claims = await _googleVerifier.VerifyAsync(request.IdToken, cancellationToken).ConfigureAwait(false);
        var now = _clock.UtcNow;

        var account = await _accounts.FindByGoogleSubAsync(claims.Subject, cancellationToken).ConfigureAwait(false);
        bool isNew = false;
        if (account is null)
        {
            string displayName = !string.IsNullOrWhiteSpace(claims.Name)
                ? claims.Name!
                : (claims.Email ?? $"player-{claims.Subject[..8]}");
            account = Account.CreateFromGoogle(claims.Subject, displayName, now);
            await _accounts.AddAsync(account, cancellationToken).ConfigureAwait(false);
            isNew = true;
        }
        else
        {
            account.TouchLogin(now);
        }

        await _accounts.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var refresh = await _refreshTokens.IssueNewFamilyAsync(account.Id, userAgent, ipAddress, cancellationToken)
            .ConfigureAwait(false);
        var access = _accessTokens.IssueAccessToken(account.Id, account.DisplayName, account.IsGuest);

        await _audit.WriteAsync(
            isNew ? AuditAction.AccountCreated : AuditAction.LoginSuccess,
            AuditSeverity.Info,
            new { provider = "google", accountId = account.Id },
            accountId: account.Id,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Google login: account {AccountId} (new={IsNew})", account.Id, isNew);

        return new AuthSessionResponse(
            account.Id,
            access.Token,
            access.ExpiresAt,
            refresh.Token,
            refresh.ExpiresAt,
            account.DisplayName,
            account.IsGuest);
    }
}
