# Idle MMORPG — Project-wide instructions for Claude

This file is the canonical context for AI agents working in this repo. Read it before doing any non-trivial change. The architecture plan lives at `~/.claude/plans/przeczytaj-assets-docs-gdd-md-oraz-effervescent-aurora.md`. The Game Design Document is `client/Assets/Docs/gdd.md`. The technical spec is `client/Assets/Docs/spec.md`.

## What this project is

Mobile (Android, portrait) idle MMORPG. Unity 6 client + .NET server hosted on the user's own VPS. Server is fully authoritative — anti-cheat is non-negotiable. Combat is turn-based ATB with shared deterministic simulation.

## Repo layout

```
idle-mmorpg/
├── client/      Unity 6 (URP mobile)
├── server/      ASP.NET Core (.NET 10)
├── shared/      .NET Standard 2.1 — deterministic combat simulation, contracts
├── tools/       CLI: content pipeline, replay-cli
└── .github/     CI workflows
```

## Hard rules

### 1. Server validates everything
The client is a renderer + input device. Never trust client-side state. Every gameplay-impacting mutation goes through a server endpoint that revalidates from authoritative state. Combat outcomes, drops, currency, level-ups, refining outcomes are all server-side. The pattern: client sends **intent** → server replays the deterministic simulation from `shared` → server commits the canonical result → client renders that.

### 2. Determinism in `shared/`
The shared library MUST produce byte-identical results on Unity (Mono/IL2CPP) and the server (CoreCLR). Hard rules:
- **No floats in core math.** Use `int` for HP/MP/damage. Multipliers are permille (`int` ‰). Use `IdleMmo.Shared.Determinism.FixedMath`.
- **No `UnityEngine.*` references** in shared library.
- **No `DateTime.*`, `System.Random`, `Guid.NewGuid`, threading, async, file I/O.** Use `IdleMmo.Shared.Determinism.IRng` (XorShift64) seeded explicitly per-battle.
- **Logical ticks only.** Time is `int Tick` (100 Hz logical). Wall-clock mapping happens in client UI code only.
- **No `Dictionary<,>` iteration** in order-sensitive code. Use sorted arrays keyed by `EntityId`.
- **No reflection or LINQ** in hot paths. Setup-time LINQ is fine.

If you change any combat formula, run the replay regression suite (`server/tests/IdleMmo.Replay.Tests`) and update goldens in the same PR. CI fails on golden divergence.

### 3. English everywhere + i18n from day one
- All code identifiers, comments, commit messages, doc files in English.
- All user-facing strings go through `Unity.Localization` (client) or `IdleMmo.Shared.Localization.LocKey` (shared/server). Never hardcode user-visible text — even placeholder strings in prefabs use a key.
- Add new strings to `client/Assets/_Project/Localization/StringTables/` immediately, English locale only at MVP.

### 4. Asmdef per subsystem
Each subdirectory under `client/Assets/_Project/Scripts/` has its own asmdef. New subsystems get a new asmdef. Cross-references are explicit. This keeps Unity recompile times reasonable.

### 5. No client-side accumulation of rewards
There is no client-side idle accumulator. The server reads `characters.last_logout_at` against server clock and computes deterministically through `IdleMmo.Shared.Progression.IdleSimulator`. Client never sends "I earned X."

### 6. Energy semantics
Never store "current energy." Store only `energy_at_full_at` (timestamp) and compute on read. This eliminates clock drift and race conditions.

### 7. Idempotency on mutating endpoints
Every POST/PUT/DELETE that mutates state accepts `X-Idem-Key`. Response is cached in Redis 24h. Replay returns the cached response. Implement it on the server side via middleware, not per-handler.

### 8. HMAC request signing
All authenticated requests carry `X-Sig` = HMAC-SHA256 of `{method}|{path}|{body_sha}|{nonce}|{ts}` keyed by a per-session secret derived during login. Nonces in Redis with 60s window. Anti-replay.

### 9. Rotating refresh tokens with reuse detection
Refresh tokens belong to a `family_id`. Each use issues a new token, marks the old one as `replaced_by_id`. Reuse of a previously-rotated token revokes the entire family — log to `audit_log severity=high`. Allow a 30-second grace window for double-submit retry.

### 10. Audit log on high-value events
Write `audit_log` rows with severity for: rare drop (Epic+), refining success above +7, PvP win, large currency change, account link, replay mismatch. Include `sim_version` and `server_seed` so old battles are forensically reproducible.

## Tech stack quick reference

| Layer | Pick |
|---|---|
| Server runtime | .NET 10 LTS |
| Server framework | ASP.NET Core (Clean Architecture) |
| ORM | EF Core 10 + Npgsql |
| Database | PostgreSQL 16 |
| Cache | Redis 7 |
| WebSocket | SignalR + MessagePack |
| BG jobs | Quartz.NET (Postgres job store) |
| Auth | Google Sign-In + guest device-hash + own JWT (HS256, rotating refresh) |
| Logging | Serilog (JSON) + OpenTelemetry → Prometheus + Grafana + Tempo |
| Reverse proxy | Caddy (Let's Encrypt automatic) |
| Tests | xUnit (server) + NUnit (shared) + Testcontainers |
| Unity | 6.0.4.6f1 + URP mobile |
| Unity DI | VContainer |
| Unity async | UniTask (Cysharp) |
| Unity SignalR | Microsoft.AspNetCore.SignalR.Client + MessagePack |
| Unity serialization | System.Text.Json + JsonSerializerContext source-gen |
| Unity localization | com.unity.localization |
| Unity MCP | com.coplaydev.unity-mcp |

## Coding conventions

- C# nullable reference types **on** in all projects (`<Nullable>enable</Nullable>`).
- File-scoped namespaces. PascalCase for types/methods, camelCase for params/locals, `_camelCase` for private fields.
- Prefer `record` for DTOs and value objects. Prefer `sealed` for classes by default.
- No public mutable static state.
- **No `async void`** except in event handlers.
- Server: one feature per `IdleMmo.Application/Features/<Feature>/` with handler + validator + DTOs co-located (vertical slice per feature inside Clean Architecture).
- Client: prefer `UniTask` over `Task` in MonoBehaviour code.

## Combat simulator specifics

- `Tick` resolution: 100 Hz logical.
- ATB charges by `speed` integer per tick. When `AtbValue >= AtbThreshold`, combatant performs basic attack; ATB resets to 0.
- Cooldowns are integer ticks. Decrement once per tick.
- Skill use is queued by client at a specific `Tick` index. Server sim consumes the queue in order and silently drops invalid uses (insufficient mana, on cooldown, dead caster, dead target on opposite team semantic mismatch).
- Loot is rolled from the same RNG stream at the moment a monster dies, in `EntityId` order. Any reordering will break determinism.
- Wave transitions: HP/MP do **not** regenerate. Cooldowns persist. The transition is a no-op tick that just advances `WaveIndex`.

## Google Sign-In on Android — gotchas

You must register **both** the **debug SHA-1** and the **release SHA-1** in Google Cloud Console for the OAuth Android client.
- Local debug SHA-1: `keytool -list -v -keystore ~/.android/debug.keystore -alias androiddebugkey -storepass android -keypass android`
- With Google Play App Signing: register the **upload key SHA-1**, then after first Play upload also register the **signing-cert SHA-1** from Play Console → App integrity.

ID tokens received from the Android client carry the `sub` claim — use it as `accounts.google_sub`.

## Running tests

```
# server unit + integration
cd server && dotnet test

# shared determinism + replay
cd shared && dotnet test

# Unity Play Mode tests are run via Unity Editor or GameCI in CI
```

## Local dev quickstart

```bash
# bring up Postgres (host port 5434) + Redis (host port 6381)
docker compose -f docker-compose.dev.yml up -d

# apply migrations
cd server && dotnet ef database update --project src/IdleMmo.Infrastructure --startup-project src/IdleMmo.Api

# run API on http://localhost:5099
dotnet run --project src/IdleMmo.Api --urls http://localhost:5099

# Unity: open client/ folder in Unity Hub (Unity 6.0.4.6f1)
```

> **dotnet-ef tool** must be on PATH: `dotnet tool install --global dotnet-ef` and add `~/.dotnet/tools` to `PATH`.

## Unity MCP setup

Unity MCP gives Claude tools to drive the Unity Editor. The package is referenced from `client/Packages/manifest.json`. Setup steps once Unity Editor is open for the first time:

1. **Open the project**: Unity Hub → Add → select `client/` → open. Unity downloads packages and compiles `_Project` scripts and the precompiled `IdleMmo.Shared.dll`.
2. **Bridge install**: the Unity MCP package surfaces a "MCP for Unity" window on first import — click *Install Bridge*. Bridge listens on a local TCP port.
3. **Claude config**: add the MCP server to `~/.claude.json` (Claude Code) or your client's MCP servers config:

   ```jsonc
   {
     "mcpServers": {
       "unity": {
         "command": "uvx",
         "args": ["mcp-server-unity"],
         "env": { "UNITY_PROJECT_PATH": "/Users/qcyqapps/Desktop/Dev/idle-mmorpg/client" }
       }
     }
   }
   ```

   Restart Claude Code to pick up the config; the `unity-mcp-skill` skill will then be effective and `mcp__unity__*` tools become callable.

> If `mcp-server-unity` is not on PyPI in your environment, check the active version of `com.coplaydev.unity-mcp` for the matching server install instructions — the package's own README is authoritative.

## Branch / commit conventions

- Branches: `slice-N-feature-name` (e.g., `slice-0-auth-google`), `fix/short-desc`, `chore/short-desc`.
- Commits: imperative mood, present tense (`add`, `fix`, `update`).
- One commit per logical change. Run tests before pushing.
- PRs that change combat formulas must update replay goldens in the same PR.
