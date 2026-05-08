using IdleMmo.Application.Common.Abstractions;
using IdleMmo.Application.Common.Time;
using IdleMmo.Domain.Entities;
using IdleMmo.Domain.Enums;
using IdleMmo.Shared.Contracts.Auth;
using Microsoft.Extensions.Logging;

namespace IdleMmo.Application.Features.Auth;

public sealed class LoginGuestHandler
{
    private readonly IAccountRepository _accounts;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IJwtTokenIssuer _accessTokens;
    private readonly IAuditLogger _audit;
    private readonly IClock _clock;
    private readonly ILogger<LoginGuestHandler> _logger;

    public LoginGuestHandler(
        IAccountRepository accounts,
        IRefreshTokenService refreshTokens,
        IJwtTokenIssuer accessTokens,
        IAuditLogger audit,
        IClock clock,
        ILogger<LoginGuestHandler> logger)
    {
        _accounts = accounts;
        _refreshTokens = refreshTokens;
        _accessTokens = accessTokens;
        _audit = audit;
        _clock = clock;
        _logger = logger;
    }

    public async Task<AuthSessionResponse> HandleAsync(
        LoginGuestRequest request,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DeviceHash);
        if (request.DeviceHash.Length is < 16 or > 128)
        {
            throw new ArgumentException("DeviceHash length must be between 16 and 128.", nameof(request));
        }

        var now = _clock.UtcNow;
        var account = await _accounts.FindByDeviceHashAsync(request.DeviceHash, cancellationToken).ConfigureAwait(false);
        bool isNew = false;
        if (account is null)
        {
            string displayName = $"Guest-{request.DeviceHash[..6]}";
            account = Account.CreateGuest(request.DeviceHash, displayName, now);
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
            new { provider = "guest", accountId = account.Id },
            accountId: account.Id,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Guest login: account {AccountId} (new={IsNew})", account.Id, isNew);

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
