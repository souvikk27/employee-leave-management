using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    // Concurrency token
    public uint Version { get; private set; }

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private User()
        : base() { }

    public User(
        Guid id,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid createdBy,
        Guid updatedBy,
        string email,
        string passwordHash,
        bool isActive
    )
        : base(id, createdAt, updatedAt, createdBy, updatedBy)
    {
        Email = email;
        PasswordHash = passwordHash;
        IsActive = isActive;
        Version = 1;
    }

    public static User Create(Guid id, Guid createdBy, string email, string passwordHash)
    {
        var now = DateTimeOffset.UtcNow;
        return new User(id, now, now, createdBy, createdBy, email, passwordHash, true);
    }

    public void UpdateEmail(string email, Guid updatedBy)
    {
        Email = email;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void UpdatePassword(string passwordHash, Guid updatedBy)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

    public void AddRole(Guid roleId, Guid createdBy)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            return;

        var userRole = UserRole.Create(Guid.NewGuid(), createdBy, Id, roleId);
        _userRoles.Add(userRole);
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = createdBy;
        Version++;
    }

    public void RemoveRole(Guid roleId, Guid updatedBy)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
            UpdatedAt = DateTimeOffset.UtcNow;
            UpdatedBy = updatedBy;
            Version++;
        }
    }

    public bool HasRole(string roleName) => _userRoles.Any(ur => ur.Role?.Name == roleName);

    public void Deactivate(Guid deactivatedBy)
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = deactivatedBy;
        Version++;
    }

    public void Activate(Guid activatedBy)
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = activatedBy;
        Version++;
    }
}
