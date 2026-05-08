#nullable enable
using Cysharp.Threading.Tasks;
using IdleMmo.Client.Core;
using UnityEngine;
using UnityEngine.Localization.Settings;
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

    [Inject]
    public void Inject(ILog log)
    {
        _log = log;
    }

    private async void Start()
    {
        _log?.Info("Boot: preloading localization...");
        var preload = LocalizationSettings.InitializationOperation;
        await preload.Task;
        _log?.Info($"Boot: localization ready, loading scene '{_nextScene}'.");
        await SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Single);
    }
}
