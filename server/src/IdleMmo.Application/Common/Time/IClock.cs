namespace IdleMmo.Application.Common.Time;

/// <summary>Abstraction over the system clock — pinned for tests.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
