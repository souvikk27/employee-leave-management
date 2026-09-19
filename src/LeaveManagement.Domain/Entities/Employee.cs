using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Domain.Entities;

public class Employee : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Concurrency token
    public uint Version { get; private set; }

    private Employee()
        : base() { }

    public Employee(
        Guid id,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid createdBy,
        Guid updatedBy,
        string name,
        Guid userId,
        bool isActive
    )
        : base(id, createdAt, updatedAt, createdBy, updatedBy)
    {
        Name = name;
        UserId = userId;
        IsActive = isActive;
        Version = 1;
    }

    public void UpdateDetails(string name, Guid updatedBy)
    {
        Name = name;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        Version++;
    }

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

    public bool CanSubmitLeave() => IsActive && User?.IsActive == true;
}
