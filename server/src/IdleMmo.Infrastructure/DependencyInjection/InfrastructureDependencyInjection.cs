using IdleMmo.Application.Common.Abstractions;
using IdleMmo.Application.Common.Time;
using IdleMmo.Infrastructure.Auth;
using IdleMmo.Infrastructure.Persistence;
using IdleMmo.Infrastructure.Persistence.Repositories;
using IdleMmo.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdleMmo.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .ValidateOnStart();

        services.AddDbContext<IdleMmoDbContext>(opt =>
        {
            string connStr = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("ConnectionStrings:Postgres is not configured.");
            opt.UseNpgsql(connStr, npg => npg.MigrationsHistoryTable("__ef_migrations_history"));
        });

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IGoogleTokenVerifier, GoogleTokenVerifier>();
        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        return services;
    }
}
