#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMmo.Client.UI.Login;

/// <summary>
/// MonoBehaviour bound to the Login screen UGUI. Pure view layer — input bubbles to the
/// presenter via events, presenter drives status text + busy state.
/// </summary>
public sealed class LoginView : MonoBehaviour, ILoginView
{
    [SerializeField] private Button _googleButton = null!;
    [SerializeField] private Button _guestButton = null!;
    [SerializeField] private TMP_Text _statusText = null!;
    [SerializeField] private CanvasGroup _busyOverlay = null!;

    public event System.Action? GoogleClicked;
    public event System.Action? GuestClicked;

    private void Awake()
    {
        _googleButton.onClick.AddListener(() => GoogleClicked?.Invoke());
        _guestButton.onClick.AddListener(() => GuestClicked?.Invoke());
    }

    public void SetStatus(string message)
    {
        _statusText.text = message;
    }

    public void SetBusy(bool busy)
    {
        _busyOverlay.alpha = busy ? 1f : 0f;
        _busyOverlay.blocksRaycasts = busy;
        _googleButton.interactable = !busy;
        _guestButton.interactable = !busy;
    }
}

public interface ILoginView
{
    event System.Action? GoogleClicked;
    event System.Action? GuestClicked;
    void SetStatus(string message);
    void SetBusy(bool busy);
}
