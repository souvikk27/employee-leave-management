namespace LeaveManagement.Application.DTOs.Dashboard;

public sealed class AdminDashboardDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int TotalRequests { get; set; }
}
