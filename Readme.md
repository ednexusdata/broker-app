### Install ef-core tool
```
dotnet tool install --global dotnet-ef
```
Then do the instructions to add it to the profile

# Migrations

### Create Migration
```
dotnet-ef migrations add InitialRequests --startup-project ../EdNexusData.Broker.Web/EdNexusData.Broker.Web.csproj --context EdNexusData.Broker.Data.PostgresDbContext --output-dir Migrations/PostgreSql
```
```
dotnet-ef migrations add InitialRequests --startup-project ../EdNexusData.Broker.Web/EdNexusData.Broker.Web.csproj --context EdNexusData.Broker.Data.MsSqlDbContext --output-dir Migrations/MsSql
```
### Remove Last Migration
```
dotnet-ef migrations remove --startup-project ../EdNexusData.Broker.Web/EdNexusData.Broker.Web.csproj --context EdNexusData.Broker.Data.PostgresDbContext
```
```
dotnet-ef migrations remove --startup-project ../EdNexusData.Broker.Web/EdNexusData.Broker.Web.csproj --context EdNexusData.Broker.Data.MsSqlDbContext
```
### Auto-apply migrations on startup
Set `DatabaseMigration:AutoMigrate` to `true` and the web app applies pending migrations (then runs the seeder) before it starts serving.
Several instances can start at once: each one takes a database lock first (`pg_advisory_lock` on PostgreSQL, `sp_getapplock` on SQL Server), so only one runs migrations and the rest wait, then find nothing left to apply.
The lock belongs to the database session, so it is released if the instance holding it crashes.
```json
"DatabaseMigration": {
  "AutoMigrate": true,
  "LockTimeoutSeconds": 600,
  "CommandTimeoutSeconds": 600
}
```
On PostgreSQL, connect directly or through a session-mode pooler. PgBouncer in transaction mode does not keep advisory locks.

### Apply to latest migration
```
dotnet ef database update
```
### Unapply migrations back to specified migration name
```
dotnet ef database update [Migration]
```
### Restore to applied first migration
```
dotnet ef database update 0
```
### Write Out Schema File (cwd EdNexusData.Broker.Data project)
```
dotnet ef migrations script --startup-project ../EdNexusData.Broker.Web/EdNexusData.Broker.Web.csproj --context EdNexusData.Broker.Data.PostgresDbContext --idempotent -o SchemaDump/PostgreSql/$(date -u +"%Y%m%d%H%M%S")_pgsql_dbschema.sql
```
```
dotnet ef migrations script --startup-project ../EdNexusData.Broker.Web/EdNexusData.Broker.Web.csproj --context EdNexusData.Broker.Data.MsSqlDbContext --idempotent -o SchemaDump/MsSql/$(date -u +"%Y%m%d%H%M%S")_mssql_dbschema.sql
```
### Install Dev Certificate
```
dotnet dev-certs https
```
