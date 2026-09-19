namespace LeaveManagement.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public Guid UpdatedBy { get; private set; }

    protected BaseEntity() { }

    protected BaseEntity(
        Guid id,
        DateTime createdAt,
        DateTime updatedAt,
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
