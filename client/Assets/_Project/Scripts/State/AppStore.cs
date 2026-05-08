namespace IdleMmo.Client.State;

/// <summary>
/// Top-level mutable application state, sliced by feature. Slice 0 only owns Auth;
/// inventory, character, combat slices are added in their respective slices.
/// </summary>
public sealed class AppStore
{
    public AuthState Auth { get; } = new();
}
