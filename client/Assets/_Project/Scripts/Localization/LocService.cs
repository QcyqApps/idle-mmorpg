#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using IdleMmo.Shared.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace IdleMmo.Client.Localization;

/// <summary>
/// Thin facade over Unity Localization. The shared layer always emits <see cref="LocKey"/>;
/// the client resolves it through the active StringTable.
/// </summary>
public sealed class LocService
{
    private const string DefaultTable = "UI";

    public async UniTask<string> ResolveAsync(LocKey key, string? table = null)
    {
        var op = LocalizationSettings.StringDatabase
            .GetTableEntryAsync(table ?? DefaultTable, key.Key);
        await op.Task;
        var entry = op.Result.Entry;
        if (entry is null)
        {
            return $"[?{key.Key}]";
        }
        if (key.Parameters is null || key.Parameters.Count == 0)
        {
            return entry.Value;
        }
        return Format(entry.Value, key.Parameters);
    }

    /// <summary>Synchronous resolve for use after preload — returns the placeholder if the table isn't loaded.</summary>
    public string ResolveSync(LocKey key, string? table = null)
    {
        var stringTable = LocalizationSettings.StringDatabase.GetTable(table ?? DefaultTable);
        if (stringTable is null) return $"[?{key.Key}]";
        var entry = stringTable.GetEntry(key.Key);
        if (entry is null) return $"[?{key.Key}]";
        if (key.Parameters is null || key.Parameters.Count == 0) return entry.Value;
        return Format(entry.Value, key.Parameters);
    }

    private static string Format(string template, IReadOnlyDictionary<string, string> parameters)
    {
        // Minimal {key}-style replacement to avoid pulling SmartFormat in slice 0.
        string result = template;
        foreach (var (k, v) in parameters)
        {
            result = result.Replace("{" + k + "}", v);
        }
        return result;
    }
}
