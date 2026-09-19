using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Application.Services;

public sealed class LeaveReviewService
{
    private readonly ILeaveCommands _commands;
    private readonly ICurrentUserService _currentUser;
    private readonly ILeaveNotifier _notifier;

    public LeaveReviewService(
        ILeaveCommands commands,
        ICurrentUserService currentUser,
        ILeaveNotifier notifier
    )
    {
        _commands = commands;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task ApproveLeaveAsync(Guid leaveId, CancellationToken cancellationToken)
    {
        var adminUserId = RequireAuthenticatedAdmin();
        var result = await _commands.ApproveLeaveAsync(adminUserId, leaveId, cancellationToken);
        await _notifier.NotifyLeaveReviewedAsync(
            result.EmployeeUserId,
            leaveId,
            "Approved",
            result.ReviewedAt,
            result.ReviewedByEmail,
            cancellationToken
        );
    }

    public async Task RejectLeaveAsync(Guid leaveId, CancellationToken cancellationToken)
    {
        var adminUserId = RequireAuthenticatedAdmin();
        var result = await _commands.RejectLeaveAsync(adminUserId, leaveId, cancellationToken);
        await _notifier.NotifyLeaveReviewedAsync(
            result.EmployeeUserId,
            leaveId,
            "Rejected",
            result.ReviewedAt,
            result.ReviewedByEmail,
            cancellationToken
        );
    }

    private Guid RequireAuthenticatedAdmin()
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAccessException("User must be authenticated");

        return _currentUser.UserId.Value;
    }
}
