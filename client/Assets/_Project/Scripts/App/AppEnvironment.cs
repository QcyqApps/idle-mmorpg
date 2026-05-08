using UnityEngine;

namespace IdleMmo.Client.App;

/// <summary>Per-environment configuration ScriptableObject. Pick one in <c>AppLifetimeScope</c>.</summary>
[CreateAssetMenu(menuName = "IdleMmo/App Environment", fileName = "AppEnvironment")]
public sealed class AppEnvironment : ScriptableObject
{
    [SerializeField] private string _environmentName = "local";
    [SerializeField] private string _apiBaseUrl = "http://localhost:5099";
    [SerializeField] private string _googleAndroidClientId = string.Empty;
    [SerializeField] private bool _verboseLogging = true;

    public string EnvironmentName => _environmentName;
    public string ApiBaseUrl => _apiBaseUrl;
    public string GoogleAndroidClientId => _googleAndroidClientId;
    public bool VerboseLogging => _verboseLogging;
}
