using LeaveManagement.Application.DTOs.Leave;
using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Application.Services;

public sealed class LeaveApplicationService
{
    private readonly ILeaveCommands _commands;
    private readonly ICurrentUserService _currentUser;

    public LeaveApplicationService(ILeaveCommands commands, ICurrentUserService currentUser)
    {
        _commands = commands;
        _currentUser = currentUser;
    }

    public Task ApplyLeaveAsync(ApplyLeaveDto dto, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAccessException("User must be authenticated");

        var userId = _currentUser.UserId!.Value;
        return _commands.ApplyLeaveAsync(userId, dto, cancellationToken);
    }
}
