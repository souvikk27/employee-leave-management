using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Domain.Entities;

public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public Guid RoleId { get; private set; }
    public Role? Role { get; private set; }

    private UserRole()
        : base() { }

    public UserRole(
        Guid id,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid createdBy,
        Guid updatedBy,
        Guid userId,
        Guid roleId
    )
        : base(id, createdAt, updatedAt, createdBy, updatedBy)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public static UserRole Create(Guid id, Guid createdBy, Guid userId, Guid roleId)
    {
        var now = DateTimeOffset.UtcNow;
        return new UserRole(id, now, now, createdBy, createdBy, userId, roleId);
    }
}
