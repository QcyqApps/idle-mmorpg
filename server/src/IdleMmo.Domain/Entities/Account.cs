namespace IdleMmo.Domain.Entities;

/// <summary>
/// A single player account. Linked to either Google (google_sub) or guest device (device_hash) — never both null.
/// One account currently maps to one character (slice 0); enforced at the Character aggregate when introduced.
/// </summary>
public sealed class Account
{
    private Account() { }

    public long Id { get; private set; }
    public string? GoogleSub { get; private set; }
    public string? DeviceHash { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastLoginAt { get; private set; }
    public bool IsBanned { get; private set; }

    public static Account CreateFromGoogle(string googleSub, string displayName, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(googleSub);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        return new Account
        {
            GoogleSub = googleSub,
            DeviceHash = null,
            DisplayName = displayName,
            CreatedAt = now,
            LastLoginAt = now,
            IsBanned = false,
        };
    }

    public static Account CreateGuest(string deviceHash, string displayName, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        return new Account
        {
            GoogleSub = null,
            DeviceHash = deviceHash,
            DisplayName = displayName,
            CreatedAt = now,
            LastLoginAt = now,
            IsBanned = false,
        };
    }

    public void TouchLogin(DateTimeOffset now) => LastLoginAt = now;

    public bool IsGuest => GoogleSub is null && DeviceHash is not null;
}
