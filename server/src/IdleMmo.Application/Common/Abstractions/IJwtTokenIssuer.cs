namespace IdleMmo.Application.Common.Abstractions;

/// <summary>Issues short-lived access JWTs for authenticated accounts.</summary>
public interface IJwtTokenIssuer
{
    AccessTokenIssuance IssueAccessToken(long accountId, string displayName, bool isGuest);
}

public sealed record AccessTokenIssuance(string Token, DateTimeOffset ExpiresAt);
