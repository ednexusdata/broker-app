using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Data;

/// <summary>
/// Applies pending EF Core migrations on startup. Multiple instances may start at the same time,
/// so a database-level lock (pg_advisory_lock / sp_getapplock) guarantees only one of them runs
/// migrations; the others wait for the lock and then find nothing left to apply.
/// The lock is tied to the database session, so it is released automatically if the holder dies.
/// </summary>
public class DatabaseMigrationService
{
    // Arbitrary but fixed key shared by every Broker instance pointing at the same database.
    private const long PostgresAdvisoryLockKey = 0x4272_6F6B_6572_4D47; // "BrokerMG"
    private const string SqlServerAppLockResource = "EdNexusData.Broker.StartupMigration";

    private readonly IConfiguration _configuration;
    private readonly BrokerDbContext _brokerDbContext;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(IConfiguration configuration, BrokerDbContext brokerDbContext, ILogger<DatabaseMigrationService> logger)
    {
        _configuration = configuration;
        _brokerDbContext = brokerDbContext;
        _logger = logger;
    }

    public bool AutoMigrateEnabled => _configuration.GetValue("DatabaseMigration:AutoMigrate", false);

    private TimeSpan LockTimeout => TimeSpan.FromSeconds(_configuration.GetValue("DatabaseMigration:LockTimeoutSeconds", 600));

    private TimeSpan CommandTimeout => TimeSpan.FromSeconds(_configuration.GetValue("DatabaseMigration:CommandTimeoutSeconds", 600));

    /// <summary>
    /// Acquires the cross-instance startup lock, runs migrations (if enabled), then runs
    /// <paramref name="whileLocked"/> (e.g. seeding) before releasing the lock.
    /// </summary>
    public async Task InvokeAsync(Func<Task>? whileLocked = null, CancellationToken cancellationToken = default)
    {
        var database = _brokerDbContext.Database;

        // Keep a single connection open so the session-scoped lock is held for the whole operation.
        await database.OpenConnectionAsync(cancellationToken);
        try
        {
            await AcquireLockAsync(cancellationToken);
            try
            {
                if (AutoMigrateEnabled)
                {
                    await MigrateAsync(cancellationToken);
                }
                else
                {
                    _logger.LogInformation("DatabaseMigration:AutoMigrate is disabled; skipping migrations.");
                }

                if (whileLocked is not null)
                {
                    await whileLocked();
                }
            }
            finally
            {
                await ReleaseLockAsync();
            }
        }
        finally
        {
            await database.CloseConnectionAsync();
        }
    }

    private async Task MigrateAsync(CancellationToken cancellationToken)
    {
        var database = _brokerDbContext.Database;

        var pending = (await database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
        {
            _logger.LogInformation("Database schema is up to date; no migrations to apply.");
            return;
        }

        _logger.LogInformation("Applying {Count} pending migration(s): {Migrations}", pending.Count, string.Join(", ", pending));

        var previousTimeout = database.GetCommandTimeout();
        database.SetCommandTimeout(CommandTimeout);
        try
        {
            await database.MigrateAsync(cancellationToken);
        }
        finally
        {
            database.SetCommandTimeout(previousTimeout);
        }

        _logger.LogInformation("Database migrations applied.");
    }

    private async Task AcquireLockAsync(CancellationToken cancellationToken)
    {
        var database = _brokerDbContext.Database;
        var timeout = LockTimeout;

        _logger.LogInformation("Acquiring database startup lock (timeout {Timeout}).", timeout);

        if (database.IsNpgsql())
        {
            // Poll with pg_try_advisory_lock so we can log progress and honor the timeout.
            var deadline = DateTime.UtcNow + timeout;
            var loggedWaiting = false;
            while (true)
            {
                var acquired = await ExecuteScalarAsync<bool>(
                    "SELECT pg_try_advisory_lock(@key)", ("@key", PostgresAdvisoryLockKey), cancellationToken);
                if (acquired) break;

                if (!loggedWaiting)
                {
                    _logger.LogInformation("Another instance holds the startup lock; waiting for it to finish.");
                    loggedWaiting = true;
                }

                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException($"Timed out after {timeout} waiting for the database startup lock.");
                }

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
        else if (database.IsSqlServer())
        {
            // sp_getapplock blocks up to @LockTimeout ms; result >= 0 means the lock was granted.
            database.SetCommandTimeout(timeout + TimeSpan.FromSeconds(30));
            var result = await ExecuteScalarAsync<int>(
                @"DECLARE @result int;
                  EXEC @result = sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = @timeout;
                  SELECT @result;",
                [("@resource", SqlServerAppLockResource), ("@timeout", (int)timeout.TotalMilliseconds)],
                cancellationToken);
            database.SetCommandTimeout(null);

            if (result < 0)
            {
                throw new TimeoutException($"Could not acquire the database startup lock (sp_getapplock returned {result}).");
            }
        }
        else
        {
            throw new NotSupportedException($"Startup lock not supported for provider {database.ProviderName}.");
        }

        _logger.LogInformation("Database startup lock acquired.");
    }

    private async Task ReleaseLockAsync()
    {
        var database = _brokerDbContext.Database;
        try
        {
            if (database.IsNpgsql())
            {
                await ExecuteScalarAsync<bool>("SELECT pg_advisory_unlock(@key)", ("@key", PostgresAdvisoryLockKey), CancellationToken.None);
            }
            else if (database.IsSqlServer())
            {
                await ExecuteScalarAsync<int>(
                    @"DECLARE @result int;
                      EXEC @result = sp_releaseapplock @Resource = @resource, @LockOwner = 'Session';
                      SELECT @result;",
                    [("@resource", SqlServerAppLockResource)],
                    CancellationToken.None);
            }
            _logger.LogInformation("Database startup lock released.");
        }
        catch (Exception ex)
        {
            // Closing the connection ends the session, which releases the lock anyway.
            _logger.LogWarning(ex, "Failed to explicitly release the database startup lock.");
        }
    }

    private Task<T> ExecuteScalarAsync<T>(string sql, (string Name, object Value) parameter, CancellationToken cancellationToken)
        => ExecuteScalarAsync<T>(sql, [parameter], cancellationToken);

    private async Task<T> ExecuteScalarAsync<T>(string sql, (string Name, object Value)[] parameters, CancellationToken cancellationToken)
    {
        DbConnection connection = _brokerDbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = _brokerDbContext.Database.GetCommandTimeout() ?? 30;
        foreach (var (name, value) in parameters)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            command.Parameters.Add(p);
        }

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (T)Convert.ChangeType(result!, typeof(T));
    }
}
