using IdleMmo.Domain.Enums;

namespace IdleMmo.Domain.Entities;

/// <summary>
/// Immutable forensic record of high-value actions: rare drops, refining successes, PvP wins,
/// auth anomalies (refresh-token reuse), large currency moves, replay mismatches.
/// Partitioned by month on <see cref="CreatedAt"/> — partition table created via raw SQL migration,
/// EF only sees the parent.
/// </summary>
public sealed class AuditLog
{
    private AuditLog() { }

    public long Id { get; private set; }
    public long? AccountId { get; private set; }
    public long? CharacterId { get; private set; }
    public AuditAction Action { get; private set; }
    public AuditSeverity Severity { get; private set; }
    public string PayloadJson { get; private set; } = "{}";
    public DateTimeOffset CreatedAt { get; private set; }
    public string? SimVersion { get; private set; }
    public long? ServerSeed { get; private set; }

    public static AuditLog Create(
        AuditAction action,
        AuditSeverity severity,
        string payloadJson,
        DateTimeOffset now,
        long? accountId = null,
        long? characterId = null,
        string? simVersion = null,
        long? serverSeed = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);
        return new AuditLog
        {
            Action = action,
            Severity = severity,
            PayloadJson = payloadJson,
            CreatedAt = now,
            AccountId = accountId,
            CharacterId = characterId,
            SimVersion = simVersion,
            ServerSeed = serverSeed,
        };
    }
}
