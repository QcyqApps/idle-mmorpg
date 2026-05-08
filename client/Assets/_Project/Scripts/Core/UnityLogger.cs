using UnityEngine;

namespace IdleMmo.Client.Core;

public interface ILog
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, System.Exception? ex = null);
}

public sealed class UnityLogger : ILog
{
    private readonly string _prefix;

    public UnityLogger(string prefix)
    {
        _prefix = prefix;
    }

    public void Info(string message) => Debug.Log($"[{_prefix}] {message}");
    public void Warn(string message) => Debug.LogWarning($"[{_prefix}] {message}");
    public void Error(string message, System.Exception? ex = null)
    {
        if (ex is null) Debug.LogError($"[{_prefix}] {message}");
        else Debug.LogError($"[{_prefix}] {message}\n{ex}");
    }
}
