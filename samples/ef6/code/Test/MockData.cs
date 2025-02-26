using Bogus;
using Model;

namespace Test;

public static class MockData
{
    private static readonly Faker Faker = new();

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
