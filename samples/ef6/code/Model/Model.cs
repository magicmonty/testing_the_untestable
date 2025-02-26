using System.Data.Entity;

namespace Model;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class AwesomeDatabase: DbContext
{
    private const string DefaultConnectionString = "Server=localhost;Database=AwesomeDatabase;User Id=sa;Password=Dev0nly!;TrustServerCertificate=True;";

    public AwesomeDatabase(): base(DefaultConnectionString) { }
    public AwesomeDatabase(string connectionString): base(connectionString) { }

    public DbSet<User> Users { get; set; } = null!;

    public IEnumerable<User> GetActiveUsers()
    {
        return Database.SqlQuery<User>("EXEC GetActiveUsers").ToList();
    }
}
