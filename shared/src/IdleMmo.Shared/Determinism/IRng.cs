namespace IdleMmo.Shared.Determinism;

/// <summary>
/// Deterministic pseudo-random number generator interface.
/// All combat / loot / progression code must consume randomness through this contract,
/// never <see cref="System.Random"/>, which is not guaranteed to be deterministic across runtimes.
/// </summary>
public interface IRng
{
    /// <summary>Advances the generator and returns the next 64-bit unsigned value.</summary>
    ulong NextUInt64();

    /// <summary>Advances the generator and returns the next 32-bit unsigned value.</summary>
    uint NextUInt32();

    /// <summary>
    /// Returns a uniformly distributed integer in <c>[minInclusive, maxExclusive)</c>.
    /// Throws <see cref="System.ArgumentOutOfRangeException"/> when the range is empty.
    /// </summary>
    int NextInt(int minInclusive, int maxExclusive);

    /// <summary>
    /// Returns true with probability <paramref name="permille"/> / 1000.
    /// Permille (‰) is preferred over floats to keep determinism.
    /// </summary>
    bool RollPermille(int permille);
}
