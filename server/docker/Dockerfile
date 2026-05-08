# syntax=docker/dockerfile:1.7

# ---------- Stage 1: build shared + server ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solutions and project files first for restore-layer caching
COPY shared/IdleMmo.Shared.sln shared/
COPY shared/src/ shared/src/
COPY shared/tests/ shared/tests/

COPY server/IdleMmo.sln server/
COPY server/src/ server/src/
COPY server/tests/ server/tests/

# Restore (skip Unity DLL copy step in container builds)
RUN dotnet restore server/IdleMmo.sln /p:SkipUnityCopy=true

# Publish (release, no self-contained, ready-to-run)
RUN dotnet publish server/src/IdleMmo.Api/IdleMmo.Api.csproj \
    -c Release \
    -o /app/api \
    --no-restore \
    /p:SkipUnityCopy=true \
    /p:UseAppHost=false

RUN dotnet publish server/src/IdleMmo.Workers/IdleMmo.Workers.csproj \
    -c Release \
    -o /app/workers \
    --no-restore \
    /p:SkipUnityCopy=true \
    /p:UseAppHost=false

# ---------- Stage 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user
RUN groupadd --system --gid 1001 idlemmo \
    && useradd --system --uid 1001 --gid idlemmo idlemmo \
    && chown -R idlemmo:idlemmo /app

USER idlemmo

# Default to API; docker-compose can override entrypoint to run the workers binary.
COPY --from=build --chown=idlemmo:idlemmo /app/api/ /app/
COPY --from=build --chown=idlemmo:idlemmo /app/workers/IdleMmo.Workers.dll /app/IdleMmo.Workers.dll
COPY --from=build --chown=idlemmo:idlemmo /app/workers/IdleMmo.Workers.runtimeconfig.json /app/IdleMmo.Workers.runtimeconfig.json
COPY --from=build --chown=idlemmo:idlemmo /app/workers/IdleMmo.Workers.deps.json /app/IdleMmo.Workers.deps.json

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "IdleMmo.Api.dll"]
