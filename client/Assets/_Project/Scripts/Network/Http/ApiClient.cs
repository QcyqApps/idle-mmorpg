#nullable enable
using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using IdleMmo.Client.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine.Networking;

namespace IdleMmo.Client.Network.Http;

public sealed class ApiClient
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        DateParseHandling = DateParseHandling.DateTimeOffset,
    };

    private readonly ApiClientOptions _options;
    private readonly ILog _log;
    private string? _accessToken;

    public ApiClient(ApiClientOptions options, ILog log)
    {
        _options = options;
        _log = log;
    }

    public void SetAccessToken(string? accessToken) => _accessToken = accessToken;
    public string? CurrentAccessToken => _accessToken;

    public UniTask<TResp> PostAsync<TReq, TResp>(string path, TReq body, CancellationToken ct = default)
        => SendAsync<TResp>(UnityWebRequest.kHttpVerbPOST, path, body, ct);

    public UniTask<TResp> GetAsync<TResp>(string path, CancellationToken ct = default)
        => SendAsync<TResp>(UnityWebRequest.kHttpVerbGET, path, default(object), ct);

    private async UniTask<TResp> SendAsync<TResp>(string method, string path, object? body, CancellationToken ct)
    {
        string url = CombineUrl(_options.BaseUrl, path);
        using var req = new UnityWebRequest(url, method);

        if (body is not null)
        {
            string json = JsonConvert.SerializeObject(body, JsonSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bytes) { contentType = "application/json" };
        }
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        if (!string.IsNullOrEmpty(_accessToken))
        {
            req.SetRequestHeader("Authorization", $"Bearer {_accessToken}");
        }

        try
        {
            await req.SendWebRequest().WithCancellation(ct);
        }
        catch (UnityWebRequestException ex)
        {
            throw TranslateError(ex.UnityWebRequest);
        }

        if (req.result != UnityWebRequest.Result.Success)
        {
            throw TranslateError(req);
        }

        string responseBody = req.downloadHandler.text;
        if (string.IsNullOrEmpty(responseBody))
        {
            return default!;
        }
        try
        {
            return JsonConvert.DeserializeObject<TResp>(responseBody, JsonSettings)!;
        }
        catch (JsonException ex)
        {
            _log.Error($"Failed to deserialize response from {path}: {responseBody}", ex);
            throw new HttpException((int)req.responseCode, "Malformed response", ex.Message);
        }
    }

    private static HttpException TranslateError(UnityWebRequest req)
    {
        int code = (int)req.responseCode;
        string body = req.downloadHandler?.text ?? string.Empty;
        ProblemDetails? problem = null;
        if (!string.IsNullOrEmpty(body))
        {
            try { problem = JsonConvert.DeserializeObject<ProblemDetails>(body, JsonSettings); }
            catch { /* not problem-details — keep raw body in detail */ }
        }
        return new HttpException(
            statusCode: code,
            title: problem?.Title,
            detail: problem?.Detail ?? body);
    }

    private static string CombineUrl(string baseUrl, string path)
    {
        if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("ApiClient.BaseUrl is empty.");
        if (baseUrl.EndsWith("/")) baseUrl = baseUrl[..^1];
        if (!path.StartsWith("/")) path = "/" + path;
        return baseUrl + path;
    }
}

public sealed class ApiClientOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5099";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(20);
}
