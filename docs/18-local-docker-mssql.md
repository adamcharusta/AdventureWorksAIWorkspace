# Local Docker SQL Server

## Purpose

This document describes the local Docker setup for SQL Server and the AdventureWorks sample database.

The setup is intended for development and testing only. It uses SQL Server Developer Edition in a Linux container and restores AdventureWorks from Microsoft's published sample backup when the database is not already present.

## Services

The root `compose.yaml` defines two services:

1. `sqlserver` - runs `mcr.microsoft.com/mssql/server:2025-latest`.
2. `adventureworks-init` - waits for SQL Server, checks whether the AdventureWorks database exists, downloads the backup if needed, detects the backup's logical file names, and restores the database.

The SQL Server data files are stored in the `sqlserver-data` Docker volume. The downloaded AdventureWorks backup is stored in the `adventureworks-backups` Docker volume.

## Required Configuration

Set a strong SQL Server administrator password before starting the services:

```powershell
$env:MSSQL_SA_PASSWORD = "ChangeThis_StrongPassword_123!"
```

Alternatively, use a local `.env` file based on `.env.example`.

The default configuration restores:

```txt
Database: AdventureWorks2025
Backup:   https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorks2025.bak
```

The init job reads logical data and log file names from the backup with `RESTORE FILELISTONLY`, so the default AdventureWorks 2025 backup does not require manual logical file name configuration.

## Start SQL Server and Restore AdventureWorks

```powershell
docker compose up -d sqlserver adventureworks-init
```

The `adventureworks-init` service is intentionally a one-shot init job. It exits successfully after the database is ready.

To inspect its logs:

```powershell
docker compose logs adventureworks-init
```

## Connect Locally

Use the host port configured by `MSSQL_PORT` in `.env.example`. The default is:

```txt
Server:   localhost,1433
User:     sa
Database: AdventureWorks2025
```

For command-line verification:

```powershell
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $env:MSSQL_SA_PASSWORD -C -Q "SELECT name FROM sys.databases WHERE name = 'AdventureWorks2025';"
```

## Re-run Behavior

On startup, the init job checks `sys.databases` for the configured AdventureWorks database name.

- If the database exists, it skips both download and restore.
- If the database does not exist and the backup file exists, it reuses the backup file.
- If neither exists, it downloads the backup and restores the database.

The restore script is defensive about database names and backup file names because those values are interpolated into SQL restore commands.

## Reset Local Data

To remove the SQL Server data and backup volumes:

```powershell
docker compose down -v
```

This deletes local container data, including the restored AdventureWorks database.

## Source References

- Microsoft Learn documents direct AdventureWorks backup downloads and Linux restore syntax in the AdventureWorks sample database guide.
- Microsoft Learn documents `mcr.microsoft.com/mssql/server:2025-latest`, `MSSQL_SA_PASSWORD`, and `/opt/mssql-tools18/bin/sqlcmd` for SQL Server Linux containers.
