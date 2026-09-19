namespace LeaveManagement.Application.Interfaces;

public interface ILeaveNotifier
{
    Task NotifyLeaveReviewedAsync(
        Guid employeeUserId,
        Guid leaveId,
        string status,
        DateTimeOffset reviewedAt,
        string reviewedByEmail,
        CancellationToken cancellationToken
    );
}
