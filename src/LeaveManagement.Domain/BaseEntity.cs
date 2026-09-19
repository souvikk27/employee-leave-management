namespace LeaveManagement.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    public Guid CreatedBy { get; private set; }

    public Guid UpdatedBy { get; protected set; }

    protected BaseEntity() { }

    protected BaseEntity(
        Guid id,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid createdBy,
        Guid updatedBy
    )
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }
}
