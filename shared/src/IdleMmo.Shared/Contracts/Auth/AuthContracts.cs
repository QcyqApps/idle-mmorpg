using System;

namespace IdleMmo.Shared.Contracts.Auth;

/// <summary>POST /auth/google — exchange a Google ID token for our own session.</summary>
public sealed record LoginGoogleRequest(string IdToken);

/// <summary>POST /auth/guest — create or resume a guest account bound to a device hash.</summary>
public sealed record LoginGuestRequest(string DeviceHash);

/// <summary>POST /auth/refresh — rotate an access + refresh token pair.</summary>
public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>Issued by the server to authenticated clients. Both tokens MUST be stored encrypted on the device.</summary>
public sealed record AuthSessionResponse(
    long AccountId,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    string DisplayName,
    bool IsGuest);
