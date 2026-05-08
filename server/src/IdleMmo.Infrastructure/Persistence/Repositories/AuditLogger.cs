using System.Text.Json;
using IdleMmo.Application.Common.Abstractions;
using IdleMmo.Application.Common.Time;
using IdleMmo.Domain.Entities;
using IdleMmo.Domain.Enums;

namespace IdleMmo.Infrastructure.Persistence.Repositories;

public sealed class AuditLogger : IAuditLogger
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    private readonly IdleMmoDbContext _db;
    private readonly IClock _clock;

    public AuditLogger(IdleMmoDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task WriteAsync(
        AuditAction action,
        AuditSeverity severity,
        object payload,
        long? accountId = null,
        long? characterId = null,
        string? simVersion = null,
        long? serverSeed = null,
        CancellationToken cancellationToken = default)
    {
        string json = JsonSerializer.Serialize(payload, Options);
        var entry = AuditLog.Create(action, severity, json, _clock.UtcNow,
            accountId, characterId, simVersion, serverSeed);
        await _db.AuditLogs.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
