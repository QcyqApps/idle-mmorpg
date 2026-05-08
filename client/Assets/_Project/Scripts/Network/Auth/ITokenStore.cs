#nullable enable
using System;

namespace IdleMmo.Client.Network.Auth;

public interface ITokenStore
{
    StoredSession? Load();
    void Save(StoredSession session);
    void Clear();
}

public sealed record StoredSession(
    long AccountId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    string DisplayName,
    bool IsGuest);
