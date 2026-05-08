namespace IdleMmo.Client.Network.Http;

/// <summary>RFC 7807 problem details, returned by the server on errors.</summary>
public sealed class ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public string? TraceId { get; set; }
}
