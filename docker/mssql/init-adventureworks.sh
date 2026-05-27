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
    exit 0
fi

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

echo "AdventureWorks database $ADVENTUREWORKS_DATABASE is ready."
