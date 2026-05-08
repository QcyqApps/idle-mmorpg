#nullable enable
using IdleMmo.Client.Core;
using IdleMmo.Client.State;
using UnityEngine;
using VContainer;

namespace IdleMmo.Client.UI;

/// <summary>
/// Slice 0 placeholder for the Hub scene. Just confirms login state on entry; the real Hub
/// (location list, character HUD, energy meter) lands in slice 3.
/// </summary>
public sealed class HubSceneEntry : MonoBehaviour
{
    private AppStore? _store;
    private ILog? _log;

    [Inject]
    public void Inject(AppStore store, ILog log)
    {
        _store = store;
        _log = log;
    }

    private void Start()
    {
        var session = _store?.Auth.Session;
        if (session is null)
        {
            _log?.Warn("Hub scene entered without an authenticated session.");
            return;
        }
        _log?.Info($"Hub: hello {session.DisplayName} (account #{session.AccountId}, guest={session.IsGuest})");
    }
}
