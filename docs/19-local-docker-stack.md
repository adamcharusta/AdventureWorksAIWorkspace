# Local Docker Stack

## Purpose

This document describes the local Docker Compose stack for running the main development services together.

The stack is intended for local development and testing only.

## Services

The root `compose.yaml` defines:

1. `seq` - local structured log viewer.
2. `sqlserver` - SQL Server 2025 Developer Edition.
3. `adventureworks-init` - one-shot AdventureWorks restore job.
4. `api` - ASP.NET Core API container.
5. `web` - React production build served by nginx.

The local SQL Server container hosts separate databases:

- `AdventureWorksAIWorkspace` for application-owned data.
- `AdventureWorks2025` for analytical AdventureWorks sample data.

## Configuration

Create a local `.env` file from `.env.example` and replace the sample passwords and signing key.

Required values:

```txt
MSSQL_SA_PASSWORD
INITIAL_ADMIN_PASSWORD
JWT_SIGNING_KEY
```

The API environment is controlled with:

```txt
ASPNETCORE_ENVIRONMENT=Development
```

Use `Production` only when you intentionally want production startup behavior, including no local Swagger UI.

Default local ports:

```txt
Web:        http://localhost:5173
API:        http://localhost:5159
Swagger:    http://localhost:5159/swagger
Seq:        http://localhost:5341
SQL Server: localhost,1433
```

## Start the Full Stack

```powershell
docker compose up -d --build
```

The first start may take longer because Docker must pull images, build the API and web images, download the AdventureWorks backup, and restore the sample database.

## Start Only Runtime Services

After images and volumes already exist:

```powershell
docker compose up -d
```

## Logs

```powershell
docker compose logs -f api
docker compose logs -f web
docker compose logs -f seq
docker compose logs -f adventureworks-init
```

The API sends Serilog events to Seq through the internal Docker network at:

```txt
http://seq
```

Seq runs without authentication in the local Compose stack by setting `SEQ_FIRSTRUN_NOAUTHENTICATION=true`.

## Health Checks

The Compose stack uses health checks to sequence local startup:

- `sqlserver` runs `sqlcmd` with `SELECT 1`.
- `seq` checks its local HTTP endpoint.
- `api` exposes `/health` and the container checks it over local HTTP.
- `web` waits for the API service to become healthy before starting.

## Stop the Stack

```powershell
docker compose down
```

To remove local databases, the downloaded AdventureWorks backup, and Seq data:

```powershell
docker compose down -v
```

## Notes

- The API defaults to `ASPNETCORE_ENVIRONMENT=Development` in Compose so Swagger is available locally.
- The API exposes `/health` for container readiness checks.
- The web app uses nginx and proxies `/api/` requests to `http://api:8080`.
- The API currently uses the `sa` login for local Compose database access. A future hardening task should add separate least-privilege logins for the application database and AdventureWorks analytical database.
- Seq authentication is disabled only for local development.
