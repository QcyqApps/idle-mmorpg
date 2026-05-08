#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using IdleMmo.Client.Core;
using IdleMmo.Client.Network.Http;
using IdleMmo.Shared.Contracts.Auth;
using UnityEngine;

namespace IdleMmo.Client.Network.Auth;

public sealed class AuthService
{
    private const string PathGoogle  = "/auth/google";
    private const string PathGuest   = "/auth/guest";
    private const string PathRefresh = "/auth/refresh";

    private readonly ApiClient _api;
    private readonly ITokenStore _store;
    private readonly ILog _log;

    public AuthService(ApiClient api, ITokenStore store, ILog log)
    {
        _api = api;
        _store = store;
        _log = log;
    }

    public StoredSession? CurrentSession { get; private set; }

    public event Action<StoredSession>? SessionChanged;
    public event Action? SessionCleared;

    public async UniTask<StoredSession> LoginGoogleAsync(string idToken, CancellationToken ct = default)
    {
        var resp = await _api.PostAsync<LoginGoogleRequest, AuthSessionResponse>(
            PathGoogle, new LoginGoogleRequest(idToken), ct);
        return Adopt(resp);
    }

    public async UniTask<StoredSession> LoginGuestAsync(string deviceHash, CancellationToken ct = default)
    {
        var resp = await _api.PostAsync<LoginGuestRequest, AuthSessionResponse>(
            PathGuest, new LoginGuestRequest(deviceHash), ct);
        return Adopt(resp);
    }

    public async UniTask<StoredSession> RefreshAsync(CancellationToken ct = default)
    {
        if (CurrentSession is null) throw new InvalidOperationException("No active session to refresh.");
        var resp = await _api.PostAsync<RefreshTokenRequest, AuthSessionResponse>(
            PathRefresh, new RefreshTokenRequest(CurrentSession.RefreshToken), ct);
        return Adopt(resp);
    }

    public bool TryRestoreFromStorage()
    {
        var s = _store.Load();
        if (s is null) return false;
        if (s.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow)
        {
            _log.Info("Stored session refresh token is expired; clearing.");
            _store.Clear();
            return false;
        }
        CurrentSession = s;
        _api.SetAccessToken(s.AccessToken);
        SessionChanged?.Invoke(s);
        return true;
    }

    public void Logout()
    {
        CurrentSession = null;
        _api.SetAccessToken(null);
        _store.Clear();
        SessionCleared?.Invoke();
    }

    /// <summary>Stable per-device hash for guest accounts (Android device id + app GUID + salt).</summary>
    public static string ComputeDeviceHash()
    {
        string raw = SystemInfo.deviceUniqueIdentifier + "|" + Application.identifier + "|idle-mmo-guest-v1";
        return Hash(raw);
    }

    private StoredSession Adopt(AuthSessionResponse resp)
    {
        var s = new StoredSession(
            resp.AccountId,
            resp.AccessToken,
            resp.AccessTokenExpiresAt,
            resp.RefreshToken,
            resp.RefreshTokenExpiresAt,
            resp.DisplayName,
            resp.IsGuest);
        CurrentSession = s;
        _api.SetAccessToken(s.AccessToken);
        _store.Save(s);
        SessionChanged?.Invoke(s);
        return s;
    }

    private static string Hash(string s)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));
        return Convert.ToHexStringLower(bytes);
    }
}
