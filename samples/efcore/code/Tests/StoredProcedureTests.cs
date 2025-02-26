using Respawn;
using static Tests.MockData;

namespace Tests;

public class StoredProcedureTests(DatabaseFixture fixture) : IAsyncLifetime, IClassFixture<DatabaseFixture>
{
    private Respawner _respawner = null!;

    public async Task InitializeAsync()
    {
        _respawner = await Respawner.CreateAsync(
            fixture.ConnectionString,
            new RespawnerOptions
            {
                TablesToIgnore = ["_EFMigrationsHistory"]
            }
        );
    }

    public async Task DisposeAsync()
    {
        await _respawner.ResetAsync(fixture.ConnectionString);
    }

    [Fact]
    public void GetActiveUsers_Returns_Only_Active_Users()
    {
        using AwesomeDatabase context = fixture.CreateContext();
        User activeUser = CreateActiveUser();
        User inactiveUser = CreateInactiveUser();

        context.Users.Add(activeUser);
        context.Users.Add(inactiveUser);

        context.SaveChanges();

        IReadOnlyCollection<ActiveUser> activeUsers = context.GetActiveUsers();

        ActiveUser user = Assert.Single(activeUsers);
        Assert.Equal(activeUser.Name, user.Name);
    }

    [Fact]
    public void GetActiveUsers_Respawn_Test()
    {
        using AwesomeDatabase context = fixture.CreateContext();
        context.Users.Add(CreateActiveUser());
        context.Users.Add(CreateInactiveUser());
        context.Users.Add(CreateActiveUser());

        context.SaveChanges();

        IReadOnlyCollection<ActiveUser> activeUsers = context.GetActiveUsers();

        Assert.Equal(2, activeUsers.Count);
    }
}
