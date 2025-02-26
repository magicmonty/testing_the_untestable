# Testing the untestable

![[img/testing_the_untestable.webp]]

## Why test stored procedures?

- **Database dependencies**: You need a database instance to test against.
  <!-- stop -->
- **Test data reset**: How do we ensure a clean state between tests?
  <!-- stop -->
- **Isolation**: We want tests that don’t interfere with each other.
  <!-- stop -->

This is where **TestContainers** and **Respawn** come into play.

## What are TestContainers and Respawn?

1. **TestContainers** ([URL](https://testcontainers.com/modules/mssql/?language=dotnet)):

   - A library for running database containers in integration tests.
   - Provides **isolated, reproducible** test environments.
   - No need for manually setting up a database.
     <!-- stop -->

2. **Respawn** ([URL](https://github.com/jbogard/Respawn)):

   - A library by **Jimmy Bogard** that helps reset database state between tests.
   - Instead of rebuilding the database, it **efficiently truncates tables**.

## Setting Up the Project

![[img/project_setup.webp]]

## Install Project Dependencies

```sh
dotnet add code/Tests package TestContainers.MsSql
dotnet add code/Tests package Respawn
dotnet add code/Tests package Bogus
```

## Creating a Stored Procedure

Let’s assume we have a stored procedure that retrieves active users.
The stored procedure is created with a common Migration in Entity Framework.

```sql
CREATE PROCEDURE [dbo].[GetActiveUsers] AS
BEGIN
  SELECT Id, Name, Email
  FROM Users
  WHERE IsActive = 1;
END
```

<!-- stop -->The Stored procedure can be accessed via a wrapper Method on the database context:

```cs
public IReadOnlyCollection<ActiveUser> GetActiveUsers()
{
    return Database.SqlQuery<ActiveUser>($"EXEC GetActiveUsers").ToList().AsReadOnly();
}
```

<!-- stop -->We need to test:

- If the stored procedure correctly returns **active users**.
  <!-- stop -->
- If it **excludes inactive users**.

## Bootstrapping the Tests

![[img/bootstrapping.webp]]

## Create a shared ClassFixture

```cs
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Tests;

public class DatabaseFixture : IAsyncLifetime
{
    public String ConnectionString { get; private set; }

    public MsSqlContainer DbContainer { get; }

    public AwesomeDatabase CreateContext() => new(ConnectionString);

    public DatabaseFixture()
    {
        DbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .WithPassword("Dev0nly!")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await DbContainer.StartAsync();
        ConnectionString = DbContainer.GetConnectionString();

        await using AwesomeDatabase context = new(ConnectionString);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContainer.DisposeAsync();
    }
}
```

## Create some Mock data

```cs
using Bogus;

namespace Tests;

public static class MockData
{
    private static readonly Faker Faker = new Faker();

    private static User CreateUser(bool isActive)
    {
        return new User
        {
            Name = Faker.Name.FullName(),
            Email = Faker.Internet.Email(),
            IsActive = isActive
        };
    }

    public static User CreateActiveUser() => CreateUser(true);
    public static User CreateInactiveUser() => CreateUser(false);
}
```

## Creating Tests

![[img/creating_tests.webp]]

## Create a first test

```cs
using Respawn;
using static Tests.MockData;

namespace Tests;

public class StoredProcedureTests: IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private Respawner _respawner;

    public StoredProcedureTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetActiveUsers_returns_only_active_users()
    {
        using AwesomeDatabase context = _fixture.CreateContext();
        User activeUser = CreateActiveUser();
        User inactiveUser = CreateInactiveUser();

        context.Users.Add(activeUser);
        context.Users.Add(inactiveUser);

        context.SaveChanges();

        IReadOnlyCollection<ActiveUser> activeUsers = context.GetActiveUsers();

        ActiveUser user = Assert.Single(activeUsers);
        Assert.Equal(activeUser.Name, user.Name);
    }
}
```

## Run the tests

Run the tests using:

```sh
dotnet test
```

<!-- stop -->Expected result: Ensures only active users are returned

## Adding a second Test

```cs
[Fact]
public void GetActiveUsers_returns_2_if_there_are_2_active_users()
{
    using AwesomeDatabase context = _fixture.CreateContext();
    context.Users.Add(CreateActiveUser());
    context.Users.Add(CreateInactiveUser());
    context.Users.Add(CreateActiveUser());

    context.SaveChanges();

    IReadOnlyCollection<ActiveUser> activeUsers = context.GetActiveUsers();

    Assert.Equal(2, activeUsers.Count);
}
```

## Run the tests again

_Expected results:_

1. **First test**: Ensures only active users are returned
2. **Second test**: Inserts multiple users, and verifies count

<!-- stop -->_Actual result:_

```text
Xunit.Sdk.EqualException
Assert.Equal() Failure: Values differ
Expected: 2
Actual:   3
   at Tests.StoredProcedureTests.GetActiveUsers_returns_2_if_there_are_2_active_users()
   ...
```

<!-- stop -->

The problem is, that we have now leftover data from the first test.

## Respawn to the rescue

Add the `IAsyncLifetime` interface to `StoredProcedureTests` and then the following functions:

```cs
public class StoredProcedureTests: IClassFixture<DatabaseFixture>, IAsyncLifetime

  ...

    public async Task InitializeAsync()
    {
        _respawner = await Respawner.CreateAsync(
            _fixture.ConnectionString,
            new RespawnerOptions
            {
                TablesToIgnore = ["_EFMigrationsHistory"]
            }
        );
    }

    public async Task DisposeAsync()
    {
        await _respawner.ResetAsync(_fixture.ConnectionString);
    }

  ...
}
```

## Lets run the tests again

Expected results:

1. **First test**: Ensures only active users are returned
2. **Second test**: Inserts multiple users, and verifies count
   <!-- stop -->
   Actual results: All tests passing

Respawn makes sure the second test doesn’t affect others.

## Benefits of This Approach

1. **Isolated Test Environment**: Each test starts fresh using TestContainers.
   <!-- stop -->
2. **Efficient Data Reset**: Respawn resets data **without recreating the DB**.
   <!-- stop -->
3. **No External Dependencies**: You don’t need a separate SQL Server instance.
   <!-- stop -->
4. **Faster Integration Testing**: Compared to traditional setup/teardown methods.

## Drawbacks / Hints

- Respawn is not compatible with .net Framework (or at least its a very old verion with a different API) - YMMV
  <!-- stop -->
  - A workaround could be to ditch Respawn and create a new DB for each Test -> **_Slow_**
    <!-- stop -->
- EF 6 works too, but without migrations you have to be creative with initializing the DbContainer
  <!-- stop -->
  - import DacPac programmatically
    <!-- stop -->
  - Script initializer manually with SQL (maybe you don't need all tables)

## **Conclusion**

We covered:
✅ Why testing stored procedures is important.<!-- stop -->
✅ How TestContainers and Respawn help in **isolated and efficient** testing.<!-- stop -->
✅ A full example with **C#, TestContainers, and Respawn**.
## Your turn

![[img/your_turn.webp]]
