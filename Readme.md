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


### Install Dev Certificate
```
dotnet dev-certs https
```
