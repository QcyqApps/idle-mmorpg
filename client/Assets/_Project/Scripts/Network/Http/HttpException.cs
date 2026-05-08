using System;

namespace IdleMmo.Client.Network.Http;

public sealed class HttpException : Exception
{
    public int StatusCode { get; }
    public string? Detail { get; }
    public string? Title { get; }

    public HttpException(int statusCode, string? title, string? detail)
        : base(title ?? $"HTTP {statusCode}")
    {
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
    }
}
