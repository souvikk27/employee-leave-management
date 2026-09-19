using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Domain.Entities;

public class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }

    public DateOnly FromDate { get; private set; }
    public DateOnly ToDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    public LeaveStatus Status { get; private set; }

    public Guid? ReviewedBy { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }

    // Concurrency token
    public uint Version { get; private set; }

    private LeaveRequest()
        : base() { }

    public LeaveRequest(
        Guid id,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid createdBy,
        Guid updatedBy,
        Guid employeeId,
        DateOnly fromDate,
        DateOnly toDate,
        string reason,
        LeaveStatus status,
        Guid? reviewedBy = null,
        DateTimeOffset? reviewedAt = null
    )
        : base(id, createdAt, updatedAt, createdBy, updatedBy)
    {
        EmployeeId = employeeId;
        FromDate = fromDate;
        ToDate = toDate;
        Reason = reason;
        Status = status;
        ReviewedBy = reviewedBy;
        ReviewedAt = reviewedAt;
        Version = 1;
    }

    public static LeaveRequest Create(
        Guid id,
        Guid createdBy,
        Guid employeeId,
        DateOnly fromDate,
        DateOnly toDate,
        string reason,
        DateTimeOffset now
    )
    {
        if (fromDate > toDate)
            throw new ArgumentException(
                "FromDate must be less than or equal to ToDate",
                nameof(fromDate)
            );

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));

        return new LeaveRequest(
            id,
            now,
            now,
            createdBy,
            createdBy,
            employeeId,
            fromDate,
            toDate,
            reason,
            LeaveStatus.Pending
        );
    }

    public void Approve(Guid adminId)
    {
        if (Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Only pending leave requests can be approved");

        Status = LeaveStatus.Approved;
        ReviewedBy = adminId;
        ReviewedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = adminId;
        Version++;
    }

    public void Reject(Guid adminId)
    {
        if (Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Only pending leave requests can be rejected");

        Status = LeaveStatus.Rejected;
        ReviewedBy = adminId;
        ReviewedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = adminId;
        Version++;
    }

    public bool OverlapsWith(LeaveRequest other)
    {
        if (other.Status == LeaveStatus.Rejected)
            return false;

        return other.FromDate <= ToDate && other.ToDate >= FromDate;
    }

    public bool IsPending => Status == LeaveStatus.Pending;
    public bool IsTerminal => Status == LeaveStatus.Approved || Status == LeaveStatus.Rejected;
}
