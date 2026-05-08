namespace IdleMmo.Domain.Entities;

/// <summary>
/// A rotating refresh token. Tokens are emitted in "families" — each rotation creates a new token
/// in the same <see cref="FamilyId"/> while marking the predecessor as <see cref="ReplacedById"/>.
/// Reuse of a token whose <see cref="ReplacedById"/> is non-null is treated as theft and revokes
/// the entire family (callers MUST also write to audit_log).
/// </summary>
public sealed class RefreshToken
{
    private RefreshToken() { }

    public long Id { get; private set; }
    public long AccountId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public long? ReplacedById { get; private set; }
    public string? UserAgent { get; private set; }
    public string? IpAddress { get; private set; }

    public bool IsRevoked => RevokedAt is not null;
    public bool IsExpired(DateTimeOffset now) => ExpiresAt <= now;
    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);

    /// <summary>Issue the very first token in a new family.</summary>
    public static RefreshToken NewFamily(long accountId, string tokenHash, DateTimeOffset now,
        TimeSpan lifetime, string? userAgent, string? ipAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        return new RefreshToken
        {
            AccountId = accountId,
            FamilyId = Guid.NewGuid(),
            TokenHash = tokenHash,
            IssuedAt = now,
            ExpiresAt = now.Add(lifetime),
            RevokedAt = null,
            ReplacedById = null,
            UserAgent = userAgent,
            IpAddress = ipAddress,
        };
    }

    /// <summary>Issue a successor token within the same family. The predecessor is rotated by the caller.</summary>
    public static RefreshToken Successor(RefreshToken predecessor, string newTokenHash, DateTimeOffset now,
        TimeSpan lifetime, string? userAgent, string? ipAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newTokenHash);
        return new RefreshToken
        {
            AccountId = predecessor.AccountId,
            FamilyId = predecessor.FamilyId,
            TokenHash = newTokenHash,
            IssuedAt = now,
            ExpiresAt = now.Add(lifetime),
            RevokedAt = null,
            ReplacedById = null,
            UserAgent = userAgent,
            IpAddress = ipAddress,
        };
    }

    public void MarkReplacedBy(long successorId, DateTimeOffset now)
    {
        if (ReplacedById is not null)
        {
            throw new InvalidOperationException("Token has already been rotated.");
        }
        ReplacedById = successorId;
        RevokedAt = now;
    }

    public void Revoke(DateTimeOffset now)
    {
        RevokedAt ??= now;
    }
}
