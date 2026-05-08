# Idle MMORPG

Mobile (Android, portrait) idle MMORPG. Unity 6 client + ASP.NET Core server with shared deterministic combat simulation. Server-authoritative anti-cheat. See [`CLAUDE.md`](./CLAUDE.md) for project-wide conventions and [`client/Assets/Docs/gdd.md`](./client/Assets/Docs/gdd.md) for game design.

## Repo layout

| Path | Purpose |
|---|---|
| `client/` | Unity 6 (URP mobile) project |
| `server/` | ASP.NET Core .NET 10 API + workers |
| `shared/` | .NET Standard 2.1 — deterministic combat simulator + DTOs (referenced by both Unity and server) |
| `tools/` | CLI helpers: content pipeline, replay diffing |

## Local dev

```bash
# bring up Postgres + Redis
docker compose -f docker-compose.dev.yml up -d

# build shared lib (auto-copies DLL into client/Assets/Plugins)
cd shared && dotnet build

# apply migrations + run API
cd ../server
dotnet ef database update --project src/IdleMmo.Infrastructure --startup-project src/IdleMmo.Api
dotnet run --project src/IdleMmo.Api

# Unity Editor
# Open the `client/` folder in Unity Hub (Unity 6.0.4.6f1).
```

## Tests

```bash
cd shared && dotnet test    # NUnit determinism + replay
cd server && dotnet test    # xUnit + Testcontainers integration
```

## Stack

.NET 10 LTS · ASP.NET Core · EF Core 10 · PostgreSQL 16 · Redis 7 · SignalR (MessagePack) · Quartz.NET · Serilog + OpenTelemetry · Unity 6 · URP · VContainer · UniTask · Unity Localization · Synty 3D
