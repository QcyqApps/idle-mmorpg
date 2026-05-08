#nullable enable
using Cysharp.Threading.Tasks;
using IdleMmo.Client.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace IdleMmo.Client.UI.Login;

/// <summary>
/// Drop on the root GameObject of the 01_Login scene. Wires the LoginView to the presenter via
/// VContainer's parent scope and transitions to the Hub on successful login.
/// </summary>
[RequireComponent(typeof(LoginView))]
public sealed class LoginSceneEntry : MonoBehaviour
{
    [SerializeField] private string _nextScene = "02_Hub";

    private LoginPresenter? _presenter;
    private ILog? _log;
    private LoginView _view = null!;

    [Inject]
    public void Inject(LoginPresenter presenter, ILog log)
    {
        _presenter = presenter;
        _log = log;
    }

    private void Awake()
    {
        _view = GetComponent<LoginView>();
    }

    private void Start()
    {
        if (_presenter is null)
        {
            Debug.LogError("LoginSceneEntry: presenter was not injected; ensure AppLifetimeScope is on the Boot scene.");
            return;
        }

        _presenter.LoggedIn += OnLoggedIn;
        _presenter.Initialize();
    }

    private void OnDestroy()
    {
        if (_presenter is not null)
        {
            _presenter.LoggedIn -= OnLoggedIn;
        }
    }

    private async void OnLoggedIn()
    {
        _log?.Info($"Login scene: navigating to {_nextScene}");
        await SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Single);
    }
}
