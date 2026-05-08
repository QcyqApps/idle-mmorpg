#nullable enable
using System;
using IdleMmo.Client.Network.Auth;

namespace IdleMmo.Client.State;

public sealed class AuthState
{
    private StoredSession? _session;

    public StoredSession? Session => _session;
    public bool IsAuthenticated => _session is not null;

    public event Action<StoredSession?>? Changed;

    public void Set(StoredSession? session)
    {
        _session = session;
        Changed?.Invoke(session);
    }
}
