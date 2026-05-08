#nullable enable
using TMPro;
using UnityEngine;

namespace IdleMmo.Client.Localization;

/// <summary>
/// Drop on any GameObject with a TMP_Text. Reads the localized string for <see cref="LocKey"/>
/// and updates the text. Re-runs on locale change.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
[DisallowMultipleComponent]
public sealed class LocalizedTextLink : MonoBehaviour
{
    [SerializeField] private string _locKey = string.Empty;
    [SerializeField] private string _table = "UI";

    private TMP_Text _label = null!;

    private void Awake()
    {
        _label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        Refresh();
        UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale _) => Refresh();

    public void SetKey(string key)
    {
        _locKey = key;
        if (isActiveAndEnabled) Refresh();
    }

    private void Refresh()
    {
        if (string.IsNullOrEmpty(_locKey)) return;
        var op = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase
            .GetLocalizedStringAsync(_table, _locKey);
        op.Completed += handle =>
        {
            if (this == null) return;
            _label.text = handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded
                ? handle.Result
                : $"[?{_locKey}]";
        };
    }
}
