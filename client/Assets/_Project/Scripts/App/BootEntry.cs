#nullable enable
using IdleMmo.Client.Core;
using IdleMmo.Client.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace IdleMmo.Client.App;

/// <summary>
/// Place on the Boot scene. After the lifetime scope is built, preload localization
/// and load the Login scene additively.
/// </summary>
public sealed class BootEntry : MonoBehaviour
{
    [SerializeField] private string _nextScene = "01_Login";

    private ILog? _log;
    private LocService? _loc;

    [Inject]
    public void Inject(ILog log, LocService loc)
    {
        _log = log;
        _loc = loc;
    }

    private async void Start()
    {
        _log?.Info("Boot: preloading localization...");
        if (_loc is not null) await _loc.InitializeAsync();
        _log?.Info($"Boot: localization ready, loading scene '{_nextScene}'.");
        await SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Single);
    }
}
