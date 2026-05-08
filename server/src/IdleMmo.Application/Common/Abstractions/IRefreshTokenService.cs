using IdleMmo.Domain.Entities;

namespace IdleMmo.Application.Common.Abstractions;

/// <summary>
/// Issues, validates, and rotates refresh tokens. Token strings are random 256-bit values returned to the client;
/// only their hash is persisted.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Mint a brand-new family for an account that just logged in.</summary>
    Task<RefreshTokenIssuance> IssueNewFamilyAsync(long accountId, string? userAgent, string? ipAddress,
        CancellationToken cancellationToken);

    /// <summary>
    /// Validate <paramref name="presentedToken"/> and rotate the family.
    /// Returns the newly issued successor and the matching <see cref="RefreshToken"/> row to update.
    /// On reuse-after-rotation: revokes the entire family and throws <see cref="RefreshTokenReuseException"/>.
    /// </summary>
    Task<RefreshTokenIssuance> RotateAsync(string presentedToken, string? userAgent, string? ipAddress,
        CancellationToken cancellationToken);
}

public sealed record RefreshTokenIssuance(
    long AccountId,
    string Token,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt);

public sealed class RefreshTokenReuseException : Exception
{
    public RefreshTokenReuseException(long accountId, Guid familyId)
        : base($"Refresh token reuse detected for account {accountId}, family {familyId}.")
    {
        AccountId = accountId;
        FamilyId = familyId;
    }

    public long AccountId { get; }
    public Guid FamilyId { get; }
}

public sealed class RefreshTokenInvalidException : Exception
{
    public RefreshTokenInvalidException(string reason) : base(reason) { }
}
