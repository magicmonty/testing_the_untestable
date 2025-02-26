using Model;
using Respawn;
using static Test.MockData;

namespace Test;

[TestClass]
public class StoredProcedureTests
{
    private static DatabaseFixture _fixture = null!;
    private Respawner _respawner = null!;

    [ClassInitialize]
    public static void SetupDatabase(TestContext testContext)
    {
        _fixture = new DatabaseFixture();
        _fixture.InitializeAsync().Wait();
    }

    [ClassCleanup]
    public static void TearDownDatabase()
    {
        _fixture.DisposeAsync().Wait();
    }

    [TestInitialize]
    public void ResetDatabase()
    {
        _respawner = Respawner.CreateAsync(_fixture.ConnectionString).Result;
    }

    [TestCleanup]
    public void CleanupDatabase()
    {
        _respawner.ResetAsync(_fixture.ConnectionString).Wait();
    }

    [TestMethod]
    public void GetActiveUsers_Returns_Only_Active_Users()
    {
        using AwesomeDatabase context = _fixture.CreateContext();
        User activeUser = CreateActiveUser();
        User inactiveUser = CreateInactiveUser();

        context.Users.Add(activeUser);
        context.Users.Add(inactiveUser);

        context.SaveChanges();

        List<User> activeUsers = context.GetActiveUsers().ToList();

        User user = Assert.ContainsSingle(activeUsers);
        Assert.AreEqual(activeUser.Name, user.Name);
    }

    [TestMethod]
    public void GetActiveUsers_Respawn_Test()
    {
        using AwesomeDatabase context = _fixture.CreateContext();
        context.Users.Add(CreateActiveUser());
        context.Users.Add(CreateInactiveUser());
        context.Users.Add(CreateActiveUser());

        context.SaveChanges();

        List<User> activeUsers = context.GetActiveUsers().ToList();

        Assert.AreEqual(2, activeUsers.Count);
    }
}
