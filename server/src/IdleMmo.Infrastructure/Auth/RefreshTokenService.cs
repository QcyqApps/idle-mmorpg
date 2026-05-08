using System.Security.Cryptography;
using System.Text.Json;
using IdleMmo.Application.Common.Abstractions;
using IdleMmo.Application.Common.Time;
using IdleMmo.Domain.Entities;
using IdleMmo.Domain.Enums;
using IdleMmo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdleMmo.Infrastructure.Auth;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IdleMmoDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly IClock _clock;
    private readonly AuthOptions _options;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IdleMmoDbContext db,
        IAuditLogger audit,
        IClock clock,
        IOptions<AuthOptions> options,
        ILogger<RefreshTokenService> logger)
    {
        _db = db;
        _audit = audit;
        _clock = clock;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RefreshTokenIssuance> IssueNewFamilyAsync(long accountId, string? userAgent, string? ipAddress,
        CancellationToken cancellationToken)
    {
        var (token, hash) = GenerateTokenAndHash();
        var now = _clock.UtcNow;
        var rt = RefreshToken.NewFamily(accountId, hash, now, _options.RefreshTokenLifetime, userAgent, ipAddress);
        await _db.RefreshTokens.AddAsync(rt, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new RefreshTokenIssuance(accountId, token, rt.IssuedAt, rt.ExpiresAt);
    }

    public async Task<RefreshTokenIssuance> RotateAsync(string presentedToken, string? userAgent, string? ipAddress,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(presentedToken);
        string presentedHash = HashToken(presentedToken);
        var now = _clock.UtcNow;

        var match = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == presentedHash, cancellationToken)
            .ConfigureAwait(false);
        if (match is null)
        {
            throw new RefreshTokenInvalidException("Refresh token not recognised.");
        }
        if (match.IsExpired(now))
        {
            throw new RefreshTokenInvalidException("Refresh token expired.");
        }

        // Reuse-after-rotation: the presented token has already been rotated. Either
        //  (a) we are inside the grace window and the caller is just retrying — accept silently, or
        //  (b) outside the grace window — TREAT AS THEFT, revoke the entire family.
        if (match.ReplacedById is not null)
        {
            var rotatedAt = match.RevokedAt ?? match.IssuedAt;
            if (now - rotatedAt <= _options.RefreshTokenRotationGrace)
            {
                _logger.LogInformation(
                    "Refresh token reused within grace window for account {AccountId}; returning existing successor",
                    match.AccountId);
                var successor = await _db.RefreshTokens
                    .FirstOrDefaultAsync(t => t.Id == match.ReplacedById.Value, cancellationToken)
                    .ConfigureAwait(false)
                    ?? throw new RefreshTokenInvalidException("Successor token not found.");
                // We can't return the original successor token (we only stored its hash), so this case still
                // needs the client to re-issue. This is a defensive log; in practice mobile clients persist the
                // successor token before discarding the old one.
                throw new RefreshTokenInvalidException(
                    "Token already rotated; client must use the most recent token issued.");
            }

            // Theft: revoke whole family.
            await RevokeFamilyAsync(match.FamilyId, now, cancellationToken).ConfigureAwait(false);
            await _audit.WriteAsync(
                AuditAction.RefreshTokenReuseDetected,
                AuditSeverity.High,
                JsonSerializer.SerializeToElement(new { accountId = match.AccountId, familyId = match.FamilyId }),
                accountId: match.AccountId,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            throw new RefreshTokenReuseException(match.AccountId, match.FamilyId);
        }

        // Happy path: rotate.
        var (newToken, newHash) = GenerateTokenAndHash();
        var successorEntity = RefreshToken.Successor(match, newHash, now, _options.RefreshTokenLifetime,
            userAgent, ipAddress);
        await _db.RefreshTokens.AddAsync(successorEntity, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        match.MarkReplacedBy(successorEntity.Id, now);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RefreshTokenIssuance(match.AccountId, newToken, successorEntity.IssuedAt, successorEntity.ExpiresAt);
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var familyTokens = await _db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (var t in familyTokens)
        {
            t.Revoke(now);
        }
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static (string Token, string Hash) GenerateTokenAndHash()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        string token = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return (token, HashToken(token));
    }

    private static string HashToken(string token)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token), hash);
        return Convert.ToHexString(hash);
    }
}
