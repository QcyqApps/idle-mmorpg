using System;

namespace IdleMmo.Shared.Determinism;

/// <summary>
/// Deterministic XorShift64 PRNG. Identical sequence on Mono, IL2CPP, and CoreCLR
/// because every operation is on <see cref="ulong"/> with explicit semantics.
/// </summary>
/// <remarks>
/// Mutable struct on purpose: combat simulation passes state by ref to avoid
/// allocations and make the RNG advance order auditable.
/// </remarks>
public struct XorShift64 : IRng
{
    private ulong _state;

    public XorShift64(ulong seed)
    {
        _state = seed == 0UL ? 0x9E3779B97F4A7C15UL : seed;
    }

    public ulong State => _state;

    public ulong NextUInt64()
    {
        ulong x = _state;
        x ^= x << 13;
        x ^= x >> 7;
        x ^= x << 17;
        _state = x;
        return x;
    }

    public uint NextUInt32() => (uint)(NextUInt64() >> 32);

    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxExclusive),
                "maxExclusive must be strictly greater than minInclusive.");
        }
        uint range = (uint)(maxExclusive - minInclusive);
        uint sample = NextUInt32() % range;
        return minInclusive + (int)sample;
    }

    public bool RollPermille(int permille)
    {
        if (permille <= 0) return false;
        if (permille >= 1000) return true;
        return NextInt(0, 1000) < permille;
    }
}
