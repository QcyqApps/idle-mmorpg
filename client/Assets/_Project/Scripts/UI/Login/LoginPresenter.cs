#nullable enable
using System;
using Cysharp.Threading.Tasks;
using IdleMmo.Client.Core;
using IdleMmo.Client.Localization;
using IdleMmo.Client.Network.Auth;
using IdleMmo.Client.Network.Http;
using IdleMmo.Client.State;
using IdleMmo.Shared.Localization;

namespace IdleMmo.Client.UI.Login;

/// <summary>
/// Plain C# presenter wired to a <see cref="ILoginView"/>. Lifetime is owned by the scene scope.
/// Slice 0 ships only the Guest button as fully functional; Google sign-in stays disabled until
/// the Android Google Identity SDK is integrated (slice 0.5).
/// </summary>
public sealed class LoginPresenter : IDisposable
{
    private readonly ILoginView _view;
    private readonly AuthService _auth;
    private readonly AppStore _store;
    private readonly LocService _loc;
    private readonly ILog _log;

    public LoginPresenter(ILoginView view, AuthService auth, AppStore store, LocService loc, ILog log)
    {
        _view = view;
        _auth = auth;
        _store = store;
        _loc = loc;
        _log = log;

        _view.GoogleClicked += OnGoogleClicked;
        _view.GuestClicked += OnGuestClicked;
    }

    public void Dispose()
    {
        _view.GoogleClicked -= OnGoogleClicked;
        _view.GuestClicked -= OnGuestClicked;
    }

    public event Action? LoggedIn;

    public void Initialize()
    {
        if (_auth.TryRestoreFromStorage())
        {
            _store.Auth.Set(_auth.CurrentSession);
            _log.Info("Restored session from storage; signaling logged in.");
            LoggedIn?.Invoke();
            return;
        }
        _view.SetStatus(_loc.ResolveSync(new LocKey("login.idle")));
    }

    private void OnGoogleClicked()
    {
        _view.SetStatus(_loc.ResolveSync(new LocKey("login.google.unavailable")));
        _log.Warn("Google Sign-In is not wired in this slice. Use Guest for now.");
    }

    private async void OnGuestClicked()
    {
        try
        {
            _view.SetBusy(true);
            _view.SetStatus(_loc.ResolveSync(new LocKey("login.guest.in_progress")));
            string deviceHash = AuthService.ComputeDeviceHash();
            var session = await _auth.LoginGuestAsync(deviceHash);
            _store.Auth.Set(session);
            _log.Info($"Guest login succeeded for account {session.AccountId}.");
            LoggedIn?.Invoke();
        }
        catch (HttpException ex)
        {
            _log.Error($"Guest login failed: {ex.StatusCode} {ex.Title}", ex);
            _view.SetStatus($"{_loc.ResolveSync(new LocKey("login.error"))} ({ex.StatusCode})");
        }
        catch (Exception ex)
        {
            _log.Error("Guest login crashed", ex);
            _view.SetStatus(_loc.ResolveSync(new LocKey("login.error")));
        }
        finally
        {
            _view.SetBusy(false);
        }
    }
}
