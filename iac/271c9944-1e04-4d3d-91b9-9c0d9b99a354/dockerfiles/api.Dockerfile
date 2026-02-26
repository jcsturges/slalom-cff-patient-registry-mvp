# ── Stage 1: Restore & Build ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore dependencies (layer-cached unless .csproj changes)
COPY ["NGR.Api.csproj", "./"]
RUN dotnet restore "NGR.Api.csproj" --runtime linux-arm64

# Build and publish
COPY . .
RUN dotnet publish "NGR.Api.csproj" \
    --configuration Release \
    --runtime linux-arm64 \
    --self-contained false \
    --output /app/publish \
    /p:UseAppHost=false

# ── Stage 2: Runtime image ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Install curl for container health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# Non-root user for security
RUN groupadd --system --gid 1001 dotnetapp \
 && useradd  --system --uid 1001 --gid dotnetapp --no-create-home dotnetapp

WORKDIR /app
COPY --from=build /app/publish .

# App listens on 8080 (non-privileged port)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Liveness check — hits the lightweight /health endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=20s --retries=3 \
  CMD curl -fsS http://localhost:8080/health || exit 1

USER dotnetapp
ENTRYPOINT ["dotnet", "NGR.Api.dll"]
