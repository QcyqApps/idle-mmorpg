using IdleMmo.Application.Features.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace IdleMmo.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginGoogleHandler>();
        services.AddScoped<LoginGuestHandler>();
        services.AddScoped<RefreshHandler>();
        return services;
    }
}
