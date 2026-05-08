#nullable enable
using IdleMmo.Client.Core;
using IdleMmo.Client.Localization;
using IdleMmo.Client.Network.Auth;
using IdleMmo.Client.Network.Http;
using IdleMmo.Client.State;
using IdleMmo.Client.UI.Login;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace IdleMmo.Client.App;

/// <summary>
/// Top-level VContainer scope. Lives on the Boot scene; subsequent scenes (Login, Hub, Battle)
/// inherit registrations.
/// </summary>
public sealed class AppLifetimeScope : LifetimeScope
{
    [SerializeField] private AppEnvironment _environment = null!;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_environment);

        builder.Register<ILog>(_ => new UnityLogger("IdleMmo"), Lifetime.Singleton);

        builder.Register<ApiClientOptions>(resolver =>
        {
            var env = resolver.Resolve<AppEnvironment>();
            return new ApiClientOptions { BaseUrl = env.ApiBaseUrl };
        }, Lifetime.Singleton);
        builder.Register<ApiClient>(Lifetime.Singleton);

        builder.Register<ITokenStore, PrefsTokenStore>(Lifetime.Singleton);
        builder.Register<AuthService>(Lifetime.Singleton);
        builder.Register<AppStore>(Lifetime.Singleton);
        builder.Register<LocService>(Lifetime.Singleton);

        // Presenters live as long as the scope they are resolved in.
        builder.Register<LoginPresenter>(Lifetime.Scoped);
    }
}
