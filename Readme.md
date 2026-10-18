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
Set `DatabaseMigration:AutoMigrate` to `true` and the web app applies pending migrations, then runs the seeder and creates the first user, before it starts serving.
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
# Connector Updates
With `ConnectorUpdates:Enabled` set to `true`, each Web and Worker process checks the connector store every `IntervalMinutes` and brings its connectors in line with the connector references added on the store page: `*` follows the latest release, any other version is pinned.
A new version is downloaded, its SHA-256 verified, installed to `connectors/<name>/<version>/` and swapped in while the app runs; work already using the old version finishes on it, then the old version is unloaded.
A version that fails to load is removed and not downloaded again; on startup, if the newest version folder does not load, older ones are tried.
```json
"ConnectorUpdates": {
  "Enabled": true,
  "IntervalMinutes": 60,
  "StoreUrl": "https://connectors.broker.ednexusdata.org"
}
```
A store release is installable only when it lists a package with an https URL and a SHA-256. Platforms use the publish workflow's names (`linux-amd64`, `linux-arm64`); a package without `Platform` works on any platform.
```json
{
  "ReferenceName": "EdNexusData.Broker.Connector.EdFiAlliance.EdFi",
  "Releases": [
    {
      "Version": "0.0.20",
      "Packages": [
        {
          "Platform": "linux-amd64",
          "Url": "https://github.com/ednexusdata/broker-connector-edfi/releases/download/edge-v0.0.20/EdNexusData.Broker.Connector.EdFiAlliance.EdFi-edge-v0.0.20-linux-amd64.tar.gz",
          "Sha256": "sha256:82958f9c4af5f930adfa8940522bae9bf99efa669ce90e39dc50b9c8683dadf1"
        }
      ]
    }
  ]
}
```
`ReferenceName` must match the connector's assembly name, and the package must contain `<ReferenceName>.dll`.

### Install Dev Certificate
```
dotnet dev-certs https
```
