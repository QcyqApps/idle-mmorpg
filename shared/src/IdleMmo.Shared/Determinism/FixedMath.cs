using System;

namespace IdleMmo.Shared.Determinism;

/// <summary>
/// Integer-only math helpers for deterministic combat math.
/// All multipliers in the game are expressed as permille (‰) to avoid floats.
/// Example: a +7.5% damage bonus is <c>1075</c> (i.e. multiply by 1075/1000).
/// </summary>
public static class FixedMath
{
    /// <summary>One thousand — the permille base.</summary>
    public const int PermilleOne = 1000;

    /// <summary>
    /// Multiplies <paramref name="value"/> by <paramref name="permille"/> / 1000 with
    /// truncated integer rounding (toward zero). Used for stat scaling, damage multipliers.
    /// </summary>
    public static int MulPermille(int value, int permille)
    {
        long product = (long)value * permille;
        return (int)(product / PermilleOne);
    }

    /// <summary>
    /// Multiplies <paramref name="value"/> by <paramref name="permille"/> / 1000 with
    /// banker's rounding (round-half-to-even is overkill for game math; we use round-half-up).
    /// </summary>
    public static int MulPermilleRounded(int value, int permille)
    {
        long product = (long)value * permille;
        long sign = product < 0 ? -1L : 1L;
        long abs = product * sign;
        long rounded = (abs + (PermilleOne / 2)) / PermilleOne;
        return (int)(rounded * sign);
    }

    /// <summary>Clamp to a signed integer range.</summary>
    public static int Clamp(int value, int min, int max)
    {
        if (max < min) throw new ArgumentException("max must be >= min");
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>
    /// Combines two permille multipliers multiplicatively: combine(1100, 1200) == 1320
    /// (i.e. +10% then +20% = +32%, not +30%).
    /// </summary>
    public static int CombinePermille(int aPermille, int bPermille)
    {
        long product = (long)aPermille * bPermille;
        return (int)(product / PermilleOne);
    }
}
