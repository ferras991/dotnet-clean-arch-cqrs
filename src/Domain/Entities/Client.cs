namespace Domain.Entities;

public sealed class Client
{
    private Client()
    {
    }

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime CreatedAt { get; private set; }

    public static Client Create(string firstName, string lastName, string email)
    {
        return new Client
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
    }
}