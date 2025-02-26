using Microsoft.EntityFrameworkCore;


public class User
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public bool IsActive { get; set; }
}

public class ActiveUser
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class AwesomeDatabase : DbContext
{
    private const string DefaultConnectionString = "Server=localhost;Database=AwesomeDatabase;User Id=sa;Password=Dev0nly!;TrustServerCertificate=True;";
    private readonly string _connectionString;

    public DbSet<User> Users { get; set; }

    public IReadOnlyCollection<ActiveUser> GetActiveUsers()
    {
        return Database.SqlQuery<ActiveUser>($"EXEC GetActiveUsers").ToList().AsReadOnly();
    }

    public AwesomeDatabase()
    {
        _connectionString = DefaultConnectionString;
    }

    public AwesomeDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(_connectionString);
}
