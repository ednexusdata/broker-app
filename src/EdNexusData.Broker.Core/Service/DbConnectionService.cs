namespace EdNexusData.Broker.Core;

public class DbConnectionService
{
    private readonly DbContext dbContext;

    public DbConnectionService(DbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<bool> IsDatabaseConnectionUpAsync()
    {
        try
        {
            await dbContext.Database.OpenConnectionAsync();
            await dbContext.Database.CloseConnectionAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ThrowIfDatabaseConnectionNotUpAsync()
    {
        if (await IsDatabaseConnectionUpAsync() == true)
        {
            return true;
        }
        else
        {
            throw new Exception($"Unable to connect to database with provider: {dbContext.Database.ProviderName}");
        }
    }
}
