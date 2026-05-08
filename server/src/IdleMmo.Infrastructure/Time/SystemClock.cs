using IdleMmo.Application.Common.Time;

namespace IdleMmo.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
