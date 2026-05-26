# syntax=docker/dockerfile:1.7
#
# Build context must be the AdventureWorksAIWorkspaceAPI/ folder
# (so that Directory.Packages.props, Directory.Build.props and global.json are available).
#
# Build:  docker build -f src/Api/Dockerfile -t adventureworks-api:dev .
# Run:    docker run --rm -p 8080:8080 adventureworks-api:dev

# ---- Stage 1: restore ----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Solution-level files needed by restore (SDK pin + Central Package Management)
COPY global.json Directory.Build.props Directory.Packages.props ./

# Project files first, so the restore layer can be cached independently of source changes
COPY src/Api/AdventureWorksAIWorkspace.Api.csproj                   src/Api/
COPY src/Application/AdventureWorksAIWorkspace.Application.csproj   src/Application/
COPY src/Domain/AdventureWorksAIWorkspace.Domain.csproj             src/Domain/
COPY src/Infrastructure/AdventureWorksAIWorkspace.Infrastructure.csproj src/Infrastructure/

RUN dotnet restore src/Api/AdventureWorksAIWorkspace.Api.csproj

# ---- Stage 2: build & publish --------------------------------------------------------
FROM restore AS publish
COPY src/ src/

RUN dotnet publish src/Api/AdventureWorksAIWorkspace.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# ---- Stage 3: runtime ----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_GENERATE_ASPNET_CERTIFICATE=false

EXPOSE 8080

COPY --from=publish /app/publish .

# Non-root user shipped with the official ASP.NET base image
USER $APP_UID

ENTRYPOINT ["dotnet", "AdventureWorksAIWorkspaceAPI.Api.dll"]
