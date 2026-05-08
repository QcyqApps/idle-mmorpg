using IdleMmo.Application.Common.Abstractions;
using IdleMmo.Application.Features.Auth;
using IdleMmo.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;

namespace IdleMmo.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/google", LoginGoogleAsync);
        group.MapPost("/guest", LoginGuestAsync);
        group.MapPost("/refresh", RefreshAsync);

        return routes;
    }

    private static async Task<IResult> LoginGoogleAsync(
        [FromBody] LoginGoogleRequest request,
        LoginGoogleHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var (ua, ip) = ExtractClientFingerprint(httpContext);
            var response = await handler.HandleAsync(request, ua, ip, cancellationToken).ConfigureAwait(false);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(title: "Invalid Google token", detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(title: "Invalid request", detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private static async Task<IResult> LoginGuestAsync(
        [FromBody] LoginGuestRequest request,
        LoginGuestHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var (ua, ip) = ExtractClientFingerprint(httpContext);
            var response = await handler.HandleAsync(request, ua, ip, cancellationToken).ConfigureAwait(false);
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(title: "Invalid request", detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private static async Task<IResult> RefreshAsync(
        [FromBody] RefreshTokenRequest request,
        RefreshHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var (ua, ip) = ExtractClientFingerprint(httpContext);
            var response = await handler.HandleAsync(request, ua, ip, cancellationToken).ConfigureAwait(false);
            return Results.Ok(response);
        }
        catch (RefreshTokenReuseException ex)
        {
            return Results.Problem(title: "Refresh token reuse detected", detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (RefreshTokenInvalidException ex)
        {
            return Results.Problem(title: "Invalid refresh token", detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(title: "Invalid request", detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private static (string? UserAgent, string? IpAddress) ExtractClientFingerprint(HttpContext ctx)
    {
        string? ua = ctx.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(ua)) ua = null;
        string? ip = ctx.Connection.RemoteIpAddress?.ToString();
        return (ua, ip);
    }
}
