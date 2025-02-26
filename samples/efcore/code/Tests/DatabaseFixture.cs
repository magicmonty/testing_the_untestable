using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Used indirectly by xUnit")]
public class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = null!;

    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
        .WithPassword("Dev0nly!")
        .Build();

    public AwesomeDatabase CreateContext() => new(ConnectionString);

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        ConnectionString = _dbContainer.GetConnectionString();

        await using AwesomeDatabase context = new(ConnectionString);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
