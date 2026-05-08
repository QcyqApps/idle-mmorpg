namespace IdleMmo.Application.Common.Abstractions;

/// <summary>Verifies a Google ID token presented by the Android client and returns the claims we trust.</summary>
public interface IGoogleTokenVerifier
{
    Task<GoogleTokenClaims> VerifyAsync(string idToken, CancellationToken cancellationToken);
}

public sealed record GoogleTokenClaims(string Subject, string? Email, string? Name);
