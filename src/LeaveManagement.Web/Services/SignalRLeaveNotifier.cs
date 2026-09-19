using LeaveManagement.Application.Interfaces;
using LeaveManagement.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LeaveManagement.Web.Services;

public sealed class SignalRLeaveNotifier : ILeaveNotifier
{
    private readonly IHubContext<LeaveNotificationsHub> _hub;
    private readonly ILogger<SignalRLeaveNotifier> _logger;

    public SignalRLeaveNotifier(
        IHubContext<LeaveNotificationsHub> hub,
        ILogger<SignalRLeaveNotifier> logger
    )
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task NotifyLeaveReviewedAsync(
        Guid employeeUserId,
        Guid leaveId,
        string status,
        DateTimeOffset reviewedAt,
        string reviewedByEmail,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _hub
                .Clients.Group(employeeUserId.ToString())
                .SendAsync(
                    "LeaveReviewed",
                    new
                    {
                        leaveId,
                        status,
                        reviewedAt,
                        reviewedByEmail,
                    },
                    cancellationToken
                );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Real-time notification for leave {LeaveId} failed", leaveId);
        }
    }
}
