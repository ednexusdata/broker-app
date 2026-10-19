using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EdNexusData.Broker.Core.Tests.Integration.Fixtures;

/// <summary>
/// An empty PostgreSQL database created for one test and dropped afterwards, on the server named by
/// ConnectionStrings:PostgreSqlConnection (appsettings.json, overridable with the
/// ConnectionStrings__PostgreSqlConnection environment variable).
/// </summary>
public sealed class TemporaryDatabase : IAsyncDisposable
{
    private readonly string _adminConnectionString;

    public string Name { get; }
    public string ConnectionString { get; }

    private TemporaryDatabase(string adminConnectionString, string name, string connectionString)
    {
        _adminConnectionString = adminConnectionString;
        Name = name;
        ConnectionString = connectionString;
    }

    public static async Task<TemporaryDatabase> CreateAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var server = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("PostgreSqlConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:PostgreSqlConnection is not configured."));

        var name = $"broker_test_{Guid.NewGuid():N}";
        var admin = new NpgsqlConnectionStringBuilder(server.ConnectionString) { Database = "postgres", Pooling = false };
        var database = new NpgsqlConnectionStringBuilder(server.ConnectionString) { Database = name };

        await using (var connection = new NpgsqlConnection(admin.ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await command.ExecuteNonQueryAsync();
        }

        return new TemporaryDatabase(admin.ConnectionString, name, database.ConnectionString);
    }

    /// <summary>Runs <paramref name="sql"/> against the server's postgres database (not this one).</summary>
    public async Task<object?> ExecuteAdminScalarAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var (parameterName, value) in parameters) command.Parameters.AddWithValue(parameterName, value);
        return await command.ExecuteScalarAsync();
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await ExecuteAdminScalarAsync($"DROP DATABASE IF EXISTS \"{Name}\" WITH (FORCE)");
    }
}
