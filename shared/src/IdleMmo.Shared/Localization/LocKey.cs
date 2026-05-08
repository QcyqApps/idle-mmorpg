using System;
using System.Collections.Generic;

namespace IdleMmo.Shared.Localization;

/// <summary>
/// A localization key + parameters bundle. The shared/server side never resolves the text —
/// it just emits keys. Client (Unity Localization) and ops dashboards do the lookup.
/// </summary>
public readonly struct LocKey : IEquatable<LocKey>
{
    public string Key { get; }
    public IReadOnlyDictionary<string, string>? Parameters { get; }

    public LocKey(string key, IReadOnlyDictionary<string, string>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Localization key must be non-empty.", nameof(key));
        }
        Key = key;
        Parameters = parameters;
    }

    public bool Equals(LocKey other) => string.Equals(Key, other.Key, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is LocKey other && Equals(other);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Key);
    public static bool operator ==(LocKey a, LocKey b) => a.Equals(b);
    public static bool operator !=(LocKey a, LocKey b) => !a.Equals(b);
    public override string ToString() => $"LocKey({Key})";
}
