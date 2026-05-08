namespace IdleMmo.Infrastructure.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>HS256 signing key. MUST be at least 32 bytes (~43 base64 chars). Provided via environment variable.</summary>
    public string JwtSigningKey { get; set; } = string.Empty;

    /// <summary>Issuer ("iss") claim — typically your API host.</summary>
    public string JwtIssuer { get; set; } = "idle-mmorpg-api";

    /// <summary>Audience ("aud") claim — your client identifier.</summary>
    public string JwtAudience { get; set; } = "idle-mmorpg-client";

    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// After rotation, the previously-rotated refresh token is still accepted within this grace window
    /// to absorb client double-submission on flaky networks.
    /// </summary>
    public TimeSpan RefreshTokenRotationGrace { get; set; } = TimeSpan.FromSeconds(30);

    public GoogleAuthOptions Google { get; set; } = new();
}

public sealed class GoogleAuthOptions
{
    /// <summary>Google OAuth Android client IDs allowed as ID-token audience.</summary>
    public List<string> AndroidClientIds { get; set; } = new();
}
