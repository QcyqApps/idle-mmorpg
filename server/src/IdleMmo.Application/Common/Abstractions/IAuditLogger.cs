using IdleMmo.Domain.Enums;

namespace IdleMmo.Application.Common.Abstractions;

public interface IAuditLogger
{
    Task WriteAsync(AuditAction action, AuditSeverity severity, object payload,
        long? accountId = null, long? characterId = null, string? simVersion = null, long? serverSeed = null,
        CancellationToken cancellationToken = default);
}
