# SQL Server Docker Setup Guide

This guide will help you set up a local SQL Server testing environment using Docker and restore your customer's backup file.

## Prerequisites

- Docker Desktop installed and running
- Docker Compose installed
- At least 4GB of available RAM for the SQL Server container

## Quick Start

### 1. Start SQL Server Container

```bash
docker-compose up -d
```

This will:
- Pull the SQL Server 2022 image (if not already available)
- Create and start a SQL Server container
- Mount your backup file directory
- Expose SQL Server on port 1433

### 2. Check Container Status

```bash
docker-compose ps
```

Wait until the container is healthy (may take 30-60 seconds):

```bash
docker-compose logs -f sqlserver
```

Press `Ctrl+C` to exit the logs.

## Restore the Database

You have two options to restore the database:

### Option A: Automatic Restore (Using Shell Script)

1. Execute the restore script:

```bash
docker exec -it moh-sqlserver bash /var/opt/mssql/scripts/restore-database.sh
```

This script will:
- Automatically detect the logical file names
- Restore the database as `eservice_new`
- Provide status updates

### Option B: Manual Restore (Using SQL Commands)

1. Connect to the SQL Server container:

```bash
docker exec -it moh-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd"
```

2. Check the backup file contents to get logical file names:

```sql
RESTORE FILELISTONLY
FROM DISK = '/var/opt/mssql/backup/eservice_new_backup_2025_11_19_223313_6907012.bak';
GO
```

3. Note the logical file names from the output (first column), then restore:

```sql
RESTORE DATABASE [eservice_new]
FROM DISK = '/var/opt/mssql/backup/eservice_new_backup_2025_11_19_223313_6907012.bak'
WITH
    MOVE 'YourDataFileName' TO '/var/opt/mssql/data/eservice_new.mdf',
    MOVE 'YourLogFileName' TO '/var/opt/mssql/data/eservice_new_log.ldf',
    REPLACE,
    RECOVERY;
GO
```

Replace `YourDataFileName` and `YourLogFileName` with the actual names from step 2.

4. Verify the restore:

```sql
SELECT name, state_desc, recovery_model_desc
FROM sys.databases
WHERE name = 'eservice_new';
GO
```

5. Exit sqlcmd:

```sql
EXIT
```

## Connection Information

Once the database is restored, you can connect using:

- **Host**: `localhost` or `127.0.0.1`
- **Port**: `1433`
- **Username**: `sa`
- **Password**: `YourStrong!Passw0rd`
- **Database**: `eservice_new`

### Connection Strings

**ADO.NET**:
```
Server=localhost,1433;Database=eservice_new;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
```

**JDBC**:
```
jdbc:sqlserver://localhost:1433;databaseName=eservice_new;user=sa;password=YourStrong!Passw0rd;encrypt=true;trustServerCertificate=true;
```

## Management Tools

You can connect to your SQL Server instance using:

- **Azure Data Studio** (recommended, cross-platform)
- **SQL Server Management Studio (SSMS)** (Windows only)
- **Visual Studio Code** with SQL Server extension
- **DBeaver** (cross-platform)

## Common Commands

### View Logs
```bash
docker-compose logs -f sqlserver
```

### Restart Container
```bash
docker-compose restart
```

### Stop Container
```bash
docker-compose down
```

### Stop and Remove All Data
```bash
docker-compose down -v
```

### Access SQL Server Shell
```bash
docker exec -it moh-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd"
```

### List All Databases
```bash
docker exec -it moh-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT name FROM sys.databases;"
```

## Troubleshooting

### Container Won't Start

1. Check if port 1433 is already in use:
   ```bash
   netstat -an | grep 1433
   ```

2. If port is in use, either stop the conflicting service or change the port in `docker-compose.yml`:
   ```yaml
   ports:
     - "1434:1433"  # Use port 1434 on host instead
   ```

### Restore Fails

1. Check the backup file exists:
   ```bash
   docker exec -it moh-sqlserver ls -lh /var/opt/mssql/backup/
   ```

2. View detailed error messages in the SQL Server logs:
   ```bash
   docker-compose logs sqlserver
   ```

3. Verify the backup file is not corrupted:
   ```bash
   docker exec -it moh-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "RESTORE HEADERONLY FROM DISK = '/var/opt/mssql/backup/eservice_new_backup_2025_11_19_223313_6907012.bak';"
   ```

### Out of Memory

If SQL Server keeps restarting, increase Docker memory allocation:
- Docker Desktop → Settings → Resources → Memory (set to at least 4GB)

## Security Notes

**IMPORTANT**: The default SA password (`YourStrong!Passw0rd`) is for local development only.

To change it:
1. Update the password in `docker-compose.yml`
2. Update the password in restore scripts
3. Restart the container:
   ```bash
   docker-compose down
   docker-compose up -d
   ```

## Data Persistence

Database files are stored in a Docker volume named `sqlserver_data`. This means:
- Data persists even if you stop/restart the container
- Data is removed if you run `docker-compose down -v`
- You can backup this volume if needed

## Additional Backups

To create a new backup of your restored database:

```bash
docker exec -it moh-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "BACKUP DATABASE [eservice_new] TO DISK = '/var/opt/mssql/backup/eservice_new_local_backup.bak' WITH INIT;"
```

The backup will be available in your `SQL-BAK` directory.
