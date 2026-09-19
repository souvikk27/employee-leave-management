using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Application.Services;

public sealed class LeaveReviewService
{
    private readonly ILeaveCommands _commands;
    private readonly ICurrentUserService _currentUser;

    public LeaveReviewService(ILeaveCommands commands, ICurrentUserService currentUser)
    {
        _commands = commands;
        _currentUser = currentUser;
    }

    public Task ApproveLeaveAsync(Guid leaveId, CancellationToken cancellationToken)
    {
        var adminUserId = RequireAuthenticatedAdmin();
        return _commands.ApproveLeaveAsync(adminUserId, leaveId, cancellationToken);
    }

    public Task RejectLeaveAsync(Guid leaveId, CancellationToken cancellationToken)
    {
        var adminUserId = RequireAuthenticatedAdmin();
        return _commands.RejectLeaveAsync(adminUserId, leaveId, cancellationToken);
    }

    private Guid RequireAuthenticatedAdmin()
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAccessException("User must be authenticated");

        return _currentUser.UserId.Value;
    }
}
