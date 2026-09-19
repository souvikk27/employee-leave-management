using LeaveManagement.Application.DTOs.Leave;

namespace LeaveManagement.Application.Interfaces;

public interface ILeaveCommands
{
    Task ApplyLeaveAsync(Guid userId, ApplyLeaveDto dto, CancellationToken cancellationToken);
    Task ApproveLeaveAsync(Guid adminUserId, Guid leaveId, CancellationToken cancellationToken);
    Task RejectLeaveAsync(Guid adminUserId, Guid leaveId, CancellationToken cancellationToken);
}
