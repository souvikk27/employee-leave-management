namespace LeaveManagement.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // Concurrency token
    public uint Version { get; private set; }

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private Role()
        : base() { }

    public Role(
        Guid id,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid createdBy,
        Guid updatedBy,
        string name,
        string description
    )
        : base(id, createdAt, updatedAt, createdBy, updatedBy)
    {
        Name = name;
        Description = description;
        Version = 1;
    }

    public static Role Create(Guid id, Guid createdBy, string name, string description)
    {
        var now = DateTimeOffset.UtcNow;
        return new Role(id, now, now, createdBy, createdBy, name, description);
    }

    public void UpdateDetails(string name, string description, Guid updatedBy)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }
}
