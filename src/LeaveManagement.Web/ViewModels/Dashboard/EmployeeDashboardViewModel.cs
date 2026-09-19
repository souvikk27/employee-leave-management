using LeaveManagement.Application.DTOs.Leave;

namespace LeaveManagement.Web.ViewModels.Dashboard;

public sealed class EmployeeDashboardViewModel
{
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int TotalRequests { get; set; }
    public IReadOnlyList<LeaveListItemDto> RecentLeaves { get; set; } =
        Array.Empty<LeaveListItemDto>();
}
