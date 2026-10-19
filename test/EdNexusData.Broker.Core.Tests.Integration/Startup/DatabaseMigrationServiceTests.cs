using EdNexusData.Broker.Core.Tests.Integration.Fixtures;
using EdNexusData.Broker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace EdNexusData.Broker.Core.Tests.Integration.Startup;

/// <summary>
/// Runs DatabaseMigrationService against a real, initially empty PostgreSQL database. Each "server" in these
/// tests has its own DbContext and connection, as separate instances starting at once would.
/// </summary>
public class DatabaseMigrationServiceTests
{
    [Fact]
    public async Task Applies_all_migrations_to_an_empty_database_when_enabled()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        await using var server = Server.Create(database, autoMigrate: true);
        var ranWhileLocked = false;

        await server.Migration.InvokeAsync(() => { ranWhileLocked = true; return Task.CompletedTask; });

        Assert.True(ranWhileLocked);
        Assert.Empty(await server.Context.Database.GetPendingMigrationsAsync());
        Assert.Equal(server.Context.Database.GetMigrations(), await server.Context.Database.GetAppliedMigrationsAsync());
    }

    [Fact]
    public async Task Skips_migrations_when_disabled_but_still_runs_the_locked_work()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        await using var server = Server.Create(database, autoMigrate: false);
        var ranWhileLocked = false;

        await server.Migration.InvokeAsync(() => { ranWhileLocked = true; return Task.CompletedTask; });

        Assert.True(ranWhileLocked);
        Assert.Empty(await server.Context.Database.GetAppliedMigrationsAsync());
    }

    [Fact]
    public async Task Is_disabled_by_default()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        await using var server = Server.Create(database, autoMigrate: null);

        Assert.False(server.Migration.AutoMigrateEnabled);
    }

    [Fact]
    public async Task Servers_starting_together_migrate_and_run_startup_work_one_at_a_time()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        const int servers = 4;
        var instances = Enumerable.Range(0, servers).Select(_ => Server.Create(database, autoMigrate: true)).ToList();
        var active = 0;
        var maxActive = 0;
        var completed = 0;

        try
        {
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var runs = instances.Select(instance => Task.Run(async () =>
            {
                await start.Task;
                await instance.Migration.InvokeAsync(async () =>
                {
                    var now = Interlocked.Increment(ref active);
                    InterlockedMax(ref maxActive, now);
                    await Task.Delay(200);
                    Interlocked.Decrement(ref active);
                    Interlocked.Increment(ref completed);
                });
            })).ToList();

            start.SetResult();
            await Task.WhenAll(runs).WaitAsync(TimeSpan.FromMinutes(2));

            Assert.Equal(servers, completed);
            Assert.Equal(1, maxActive);
            Assert.Empty(await instances[0].Context.Database.GetPendingMigrationsAsync());
        }
        finally
        {
            foreach (var instance in instances) await instance.DisposeAsync();
        }
    }

    [Fact]
    public async Task Waiting_server_gives_up_after_the_lock_timeout()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        await using var holder = Server.Create(database, autoMigrate: false);
        await using var waiter = Server.Create(database, autoMigrate: false, lockTimeoutSeconds: 1);
        var holding = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var holderRun = holder.Migration.InvokeAsync(async () => { holding.SetResult(); await release.Task; });
        await holding.Task.WaitAsync(TimeSpan.FromSeconds(30));

        await Assert.ThrowsAsync<TimeoutException>(() => waiter.Migration.InvokeAsync());

        release.SetResult();
        await holderRun;
    }

    [Fact]
    public async Task Lock_is_released_when_the_startup_work_fails()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        await using var failing = Server.Create(database, autoMigrate: false);
        await using var next = Server.Create(database, autoMigrate: false, lockTimeoutSeconds: 5);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            failing.Migration.InvokeAsync(() => throw new InvalidOperationException("seeding failed")));

        var ran = false;
        await next.Migration.InvokeAsync(() => { ran = true; return Task.CompletedTask; });
        Assert.True(ran);
    }

    [Fact]
    public async Task Lock_is_released_when_the_server_holding_it_dies()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        await using var crashing = Server.Create(database, autoMigrate: false);
        await using var next = Server.Create(database, autoMigrate: false, lockTimeoutSeconds: 10);
        var holding = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var crashingRun = crashing.Migration.InvokeAsync(async () =>
        {
            var backendId = await crashing.Context.Database.SqlQueryRaw<int>("SELECT pg_backend_pid() AS \"Value\"").SingleAsync();
            holding.SetResult(backendId);
            await release.Task;
        });
        var backend = await holding.Task.WaitAsync(TimeSpan.FromSeconds(30));

        // Simulate the holder crashing: its database session ends without releasing the lock.
        await database.ExecuteAdminScalarAsync("SELECT pg_terminate_backend(@pid)", ("pid", backend));

        var ran = false;
        await next.Migration.InvokeAsync(() => { ran = true; return Task.CompletedTask; });
        Assert.True(ran);

        release.SetResult();
        await Record.ExceptionAsync(() => crashingRun);
    }

    /// <summary>One "server": its own configuration, DbContext (and so connection) and migration service.</summary>
    private sealed class Server : IAsyncDisposable
    {
        public required PostgresDbContext Context { get; init; }
        public required DatabaseMigrationService Migration { get; init; }

        public static Server Create(TemporaryDatabase database, bool? autoMigrate, int? lockTimeoutSeconds = null)
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSqlConnection"] = database.ConnectionString,
            };
            if (autoMigrate is not null) settings["DatabaseMigration:AutoMigrate"] = autoMigrate.Value.ToString();
            if (lockTimeoutSeconds is not null) settings["DatabaseMigration:LockTimeoutSeconds"] = lockTimeoutSeconds.Value.ToString();

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            var context = new PostgresDbContext(configuration);
            return new Server
            {
                Context = context,
                Migration = new DatabaseMigrationService(configuration, context, NullLogger<DatabaseMigrationService>.Instance),
            };
        }

        public ValueTask DisposeAsync() => Context.DisposeAsync();
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        while (value > (current = Volatile.Read(ref target)) && Interlocked.CompareExchange(ref target, value, current) != current)
        {
        }
    }
}
