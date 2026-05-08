using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IdleMmo.Application.Common.Abstractions;
using IdleMmo.Infrastructure.Auth;
using IdleMmo.Shared.Contracts.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdleMmo.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IdleMmo.Api.IntegrationTests;

public sealed class AuthFlowTests : IAsyncLifetime
{
    private const string SigningKeyMaterial = "test_signing_key_at_least_32_bytes_long_for_hs256_aaaa";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("idlemmo_test")
        .WithUsername("idlemmo")
        .WithPassword("test_password")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                    ["Auth:JwtSigningKey"] = SigningKeyMaterial,
                    ["Auth:JwtIssuer"] = "idle-mmorpg-api-test",
                    ["Auth:JwtAudience"] = "idle-mmorpg-client-test",
                    ["Auth:AccessTokenLifetime"] = "00:15:00",
                    ["Auth:RefreshTokenLifetime"] = "30.00:00:00",
                    ["Auth:RefreshTokenRotationGrace"] = "00:00:30",
                });
            });
        });
        _client = _factory.CreateClient();

        // apply migrations
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdleMmoDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task GuestLogin_FirstCall_CreatesAccountAndIssuesTokens()
    {
        var resp = await _client.PostAsJsonAsync("/auth/guest",
            new LoginGuestRequest("a".PadRight(64, 'b')));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await resp.Content.ReadFromJsonAsync<AuthSessionResponse>();
        session.Should().NotBeNull();
        session!.AccountId.Should().BeGreaterThan(0);
        session.AccessToken.Should().NotBeNullOrWhiteSpace();
        session.RefreshToken.Should().NotBeNullOrWhiteSpace();
        session.IsGuest.Should().BeTrue();
    }

    [Fact]
    public async Task GuestLogin_SameDevice_ReturnsExistingAccount()
    {
        var hash = "device-hash-stable-test".PadRight(48, 'x');
        var first = await _client.PostAsJsonAsync("/auth/guest", new LoginGuestRequest(hash));
        first.EnsureSuccessStatusCode();
        var firstSession = (await first.Content.ReadFromJsonAsync<AuthSessionResponse>())!;

        var second = await _client.PostAsJsonAsync("/auth/guest", new LoginGuestRequest(hash));
        second.EnsureSuccessStatusCode();
        var secondSession = (await second.Content.ReadFromJsonAsync<AuthSessionResponse>())!;

        secondSession.AccountId.Should().Be(firstSession.AccountId);
        secondSession.RefreshToken.Should().NotBe(firstSession.RefreshToken);
    }

    [Fact]
    public async Task Refresh_RotatesToken_AndSubsequentReuseIsRejected()
    {
        var hash = "device-hash-rotation-test".PadRight(48, 'y');
        var login = await _client.PostAsJsonAsync("/auth/guest", new LoginGuestRequest(hash));
        login.EnsureSuccessStatusCode();
        var s1 = (await login.Content.ReadFromJsonAsync<AuthSessionResponse>())!;

        var rotate1 = await _client.PostAsJsonAsync("/auth/refresh",
            new RefreshTokenRequest(s1.RefreshToken));
        rotate1.StatusCode.Should().Be(HttpStatusCode.OK);
        var s2 = (await rotate1.Content.ReadFromJsonAsync<AuthSessionResponse>())!;
        s2.RefreshToken.Should().NotBe(s1.RefreshToken);

        // Reuse old token within grace window — should fail with 401, not silently succeed.
        var reuse = await _client.PostAsJsonAsync("/auth/refresh",
            new RefreshTokenRequest(s1.RefreshToken));
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GuestLogin_BadDeviceHash_Returns400()
    {
        var resp = await _client.PostAsJsonAsync("/auth/guest", new LoginGuestRequest("short"));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Refresh_UnknownToken_Returns401()
    {
        var resp = await _client.PostAsJsonAsync("/auth/refresh",
            new RefreshTokenRequest("totally-not-a-real-token-aaaaaa"));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
