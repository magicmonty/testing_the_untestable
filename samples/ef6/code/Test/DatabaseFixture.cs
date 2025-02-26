using Microsoft.SqlServer.Dac;
using Model;
using Testcontainers.MsSql;

namespace Test;

public class DatabaseFixture
{
    private const string DacpacPath = "Database.dacpac";

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

        using AwesomeDatabase context = new(ConnectionString);
        context.Database.CreateIfNotExists();
        DeployDacpac(DacpacPath);
    }

    private void DeployDacpac(string dacpacPath)
    {
            DacServices dacServices = new(ConnectionString);

            using DacPackage dacpac = DacPackage.Load(dacpacPath);

            DacDeployOptions deployOptions = new()
            {
                BlockOnPossibleDataLoss = false, // Set to true if you want to prevent data loss
                CreateNewDatabase = false        // Set to true if you want to create a new database
            };

            dacServices.Deploy(dacpac, "master", true, deployOptions);
    }
    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
