#!/usr/bin/env bash
set -euo pipefail

SQLSERVER_HOST="${SQLSERVER_HOST:-sqlserver}"
SQLSERVER_PORT="${SQLSERVER_PORT:-1433}"
MSSQL_SA_PASSWORD="${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is required}"

ADVENTUREWORKS_DATABASE="${ADVENTUREWORKS_DATABASE:-AdventureWorks2025}"
ADVENTUREWORKS_BACKUP_URL="${ADVENTUREWORKS_BACKUP_URL:-https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorks2025.bak}"
ADVENTUREWORKS_BACKUP_FILE="${ADVENTUREWORKS_BACKUP_FILE:-AdventureWorks2025.bak}"
ADVENTUREWORKS_DATA_LOGICAL_NAME="${ADVENTUREWORKS_DATA_LOGICAL_NAME:-}"
ADVENTUREWORKS_LOG_LOGICAL_NAME="${ADVENTUREWORKS_LOG_LOGICAL_NAME:-}"

# Least-privilege, read-only login used by the application to query AdventureWorks.
ADVENTUREWORKS_READER_USER="${ADVENTUREWORKS_READER_USER:-awreader}"
ADVENTUREWORKS_READER_PASSWORD="${ADVENTUREWORKS_READER_PASSWORD:?ADVENTUREWORKS_READER_PASSWORD is required}"

# Application database and its dedicated owner login (full access scoped to this database only).
APP_DATABASE="${APP_DATABASE:-AdventureWorksAIWorkspace}"
APP_DB_USER="${APP_DB_USER:-awapp}"
APP_DB_PASSWORD="${APP_DB_PASSWORD:?APP_DB_PASSWORD is required}"

BACKUP_DIR="${ADVENTUREWORKS_BACKUP_DIR:-/var/opt/mssql/backup}"
DATA_DIR="${MSSQL_DATA_DIR:-/var/opt/mssql/data}"
BACKUP_PATH="$BACKUP_DIR/$ADVENTUREWORKS_BACKUP_FILE"

SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
SQLCMD_ARGS=(-S "$SQLSERVER_HOST,$SQLSERVER_PORT" -U sa -P "$MSSQL_SA_PASSWORD" -C -b -r 1)

validate_identifier() {
    local name="$1"
    local value="$2"

    if [[ ! "$value" =~ ^[A-Za-z0-9_]+$ ]]; then
        echo "$name can contain only letters, digits, and underscores. Received: $value" >&2
        exit 1
    fi
}

validate_file_name() {
    local name="$1"
    local value="$2"

    if [[ ! "$value" =~ ^[A-Za-z0-9._-]+$ ]]; then
        echo "$name can contain only letters, digits, dots, hyphens, and underscores. Received: $value" >&2
        exit 1
    fi
}

validate_identifier "ADVENTUREWORKS_DATABASE" "$ADVENTUREWORKS_DATABASE"
validate_identifier "ADVENTUREWORKS_READER_USER" "$ADVENTUREWORKS_READER_USER"
validate_identifier "APP_DATABASE" "$APP_DATABASE"
validate_identifier "APP_DB_USER" "$APP_DB_USER"
validate_file_name "ADVENTUREWORKS_BACKUP_FILE" "$ADVENTUREWORKS_BACKUP_FILE"

if [ -n "$ADVENTUREWORKS_DATA_LOGICAL_NAME" ]; then
    validate_identifier "ADVENTUREWORKS_DATA_LOGICAL_NAME" "$ADVENTUREWORKS_DATA_LOGICAL_NAME"
fi

if [ -n "$ADVENTUREWORKS_LOG_LOGICAL_NAME" ]; then
    validate_identifier "ADVENTUREWORKS_LOG_LOGICAL_NAME" "$ADVENTUREWORKS_LOG_LOGICAL_NAME"
fi

echo "Waiting for SQL Server at $SQLSERVER_HOST,$SQLSERVER_PORT..."
for attempt in $(seq 1 60); do
    if "$SQLCMD" "${SQLCMD_ARGS[@]}" -Q "SET NOCOUNT ON; SELECT 1;" >/dev/null 2>&1; then
        echo "SQL Server is ready."
        break
    fi

    if [ "$attempt" -eq 60 ]; then
        echo "SQL Server did not become ready in time." >&2
        exit 1
    fi

    sleep 2
done

database_exists=$(
    "$SQLCMD" "${SQLCMD_ARGS[@]}" -h -1 -W \
        -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$ADVENTUREWORKS_DATABASE') IS NULL THEN 0 ELSE 1 END;" \
    | tr -d '\r' \
    | awk 'NF { print $1; exit }'
)

if [ "$database_exists" = "1" ]; then
    echo "Database $ADVENTUREWORKS_DATABASE already exists. AdventureWorks restore is skipped."
else

mkdir -p "$BACKUP_DIR"

if [ ! -f "$BACKUP_PATH" ]; then
    echo "Downloading AdventureWorks backup from $ADVENTUREWORKS_BACKUP_URL..."
    curl --fail --location --retry 5 --retry-delay 5 \
        --output "$BACKUP_PATH.tmp" \
        "$ADVENTUREWORKS_BACKUP_URL"
    mv "$BACKUP_PATH.tmp" "$BACKUP_PATH"
else
    echo "Backup file already exists at $BACKUP_PATH. Download is skipped."
fi

chmod 0644 "$BACKUP_PATH"

if [ -z "$ADVENTUREWORKS_DATA_LOGICAL_NAME" ] || [ -z "$ADVENTUREWORKS_LOG_LOGICAL_NAME" ]; then
    echo "Detecting logical file names from backup..."
    file_list=$(
        "$SQLCMD" "${SQLCMD_ARGS[@]}" -h -1 -W -s "|" \
            -Q "RESTORE FILELISTONLY FROM DISK = N'$BACKUP_PATH';" \
        | tr -d '\r'
    )

    detected_data_logical_name=$(
        printf '%s\n' "$file_list" \
        | awk -F'|' '$3 ~ /^[[:space:]]*D[[:space:]]*$/ { gsub(/^[ \t]+|[ \t]+$/, "", $1); print $1; exit }'
    )

    detected_log_logical_name=$(
        printf '%s\n' "$file_list" \
        | awk -F'|' '$3 ~ /^[[:space:]]*L[[:space:]]*$/ { gsub(/^[ \t]+|[ \t]+$/, "", $1); print $1; exit }'
    )

    ADVENTUREWORKS_DATA_LOGICAL_NAME="${ADVENTUREWORKS_DATA_LOGICAL_NAME:-$detected_data_logical_name}"
    ADVENTUREWORKS_LOG_LOGICAL_NAME="${ADVENTUREWORKS_LOG_LOGICAL_NAME:-$detected_log_logical_name}"

    validate_identifier "ADVENTUREWORKS_DATA_LOGICAL_NAME" "$ADVENTUREWORKS_DATA_LOGICAL_NAME"
    validate_identifier "ADVENTUREWORKS_LOG_LOGICAL_NAME" "$ADVENTUREWORKS_LOG_LOGICAL_NAME"
fi

echo "Using logical data file: $ADVENTUREWORKS_DATA_LOGICAL_NAME"
echo "Using logical log file: $ADVENTUREWORKS_LOG_LOGICAL_NAME"

restore_sql="$(mktemp)"
cat > "$restore_sql" <<SQL
USE [master];
GO

IF DB_ID(N'$ADVENTUREWORKS_DATABASE') IS NULL
BEGIN
    RESTORE DATABASE [$ADVENTUREWORKS_DATABASE]
    FROM DISK = N'$BACKUP_PATH'
    WITH
        MOVE N'$ADVENTUREWORKS_DATA_LOGICAL_NAME' TO N'$DATA_DIR/${ADVENTUREWORKS_DATABASE}_Data.mdf',
        MOVE N'$ADVENTUREWORKS_LOG_LOGICAL_NAME' TO N'$DATA_DIR/${ADVENTUREWORKS_DATABASE}_Log.ldf',
        FILE = 1,
        NOUNLOAD,
        STATS = 5;
END
GO
SQL

echo "Restoring $ADVENTUREWORKS_DATABASE from $BACKUP_PATH..."
"$SQLCMD" "${SQLCMD_ARGS[@]}" -i "$restore_sql"
rm -f "$restore_sql"

echo "AdventureWorks database $ADVENTUREWORKS_DATABASE restored."
fi

# Ensure a least-privilege, read-only login exists for the application's AdventureWorks
# connection. db_datareader grants SELECT only; the explicit DENY blocks data modification even
# if a future role grant would otherwise allow it. This is defense-in-depth on top of the
# application's SQL safety validator, so AI-generated SQL can never write to AdventureWorks.
# Runs on every start (idempotent), including when the database was restored previously.
reader_password_escaped=${ADVENTUREWORKS_READER_PASSWORD//\'/\'\'}
reader_sql="$(mktemp)"
cat > "$reader_sql" <<SQL
USE [master];
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$ADVENTUREWORKS_READER_USER')
    CREATE LOGIN [$ADVENTUREWORKS_READER_USER] WITH PASSWORD = N'$reader_password_escaped';
ELSE
    ALTER LOGIN [$ADVENTUREWORKS_READER_USER] WITH PASSWORD = N'$reader_password_escaped';
GO
USE [$ADVENTUREWORKS_DATABASE];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$ADVENTUREWORKS_READER_USER')
    CREATE USER [$ADVENTUREWORKS_READER_USER] FOR LOGIN [$ADVENTUREWORKS_READER_USER];
GO
IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members AS drm
    JOIN sys.database_principals AS r ON r.principal_id = drm.role_principal_id AND r.name = N'db_datareader'
    JOIN sys.database_principals AS m ON m.principal_id = drm.member_principal_id AND m.name = N'$ADVENTUREWORKS_READER_USER'
)
    ALTER ROLE db_datareader ADD MEMBER [$ADVENTUREWORKS_READER_USER];
GO
DENY INSERT, UPDATE, DELETE TO [$ADVENTUREWORKS_READER_USER];
GO
SQL

echo "Configuring read-only login '$ADVENTUREWORKS_READER_USER' on $ADVENTUREWORKS_DATABASE..."
"$SQLCMD" "${SQLCMD_ARGS[@]}" -i "$reader_sql"
rm -f "$reader_sql"

# Ensure the application database exists and is owned by a dedicated login. The application gets
# full access (db_owner) to its own database only, so EF Core migrations work, while the login has
# no server-level rights and no access to AdventureWorks. Idempotent; preserves existing data.
app_password_escaped=${APP_DB_PASSWORD//\'/\'\'}
app_sql="$(mktemp)"
cat > "$app_sql" <<SQL
USE [master];
GO
IF DB_ID(N'$APP_DATABASE') IS NULL
    CREATE DATABASE [$APP_DATABASE];
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$APP_DB_USER')
    CREATE LOGIN [$APP_DB_USER] WITH PASSWORD = N'$app_password_escaped';
ELSE
    ALTER LOGIN [$APP_DB_USER] WITH PASSWORD = N'$app_password_escaped';
GO
USE [$APP_DATABASE];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$APP_DB_USER')
    CREATE USER [$APP_DB_USER] FOR LOGIN [$APP_DB_USER];
GO
IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members AS drm
    JOIN sys.database_principals AS r ON r.principal_id = drm.role_principal_id AND r.name = N'db_owner'
    JOIN sys.database_principals AS m ON m.principal_id = drm.member_principal_id AND m.name = N'$APP_DB_USER'
)
    ALTER ROLE db_owner ADD MEMBER [$APP_DB_USER];
GO
SQL

echo "Configuring application owner login '$APP_DB_USER' on $APP_DATABASE..."
"$SQLCMD" "${SQLCMD_ARGS[@]}" -i "$app_sql"
rm -f "$app_sql"

echo "Database bootstrap complete. AdventureWorks database $ADVENTUREWORKS_DATABASE is ready."
