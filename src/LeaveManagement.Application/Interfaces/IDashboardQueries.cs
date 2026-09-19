using LeaveManagement.Application.DTOs.Dashboard;

namespace LeaveManagement.Application.Interfaces;

public interface IDashboardQueries
{
    Task<AdminDashboardDto> GetAdminSummaryAsync(CancellationToken cancellationToken);
    Task<EmployeeDashboardDto> GetEmployeeSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken
    );
}
