using LeaveManagement.Application.DTOs.Dashboard;
using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Application.Services;

public sealed class DashboardApplicationService
{
    private readonly IDashboardQueries _queries;
    private readonly ICurrentUserService _currentUser;

    public DashboardApplicationService(IDashboardQueries queries, ICurrentUserService currentUser)
    {
        _queries = queries;
        _currentUser = currentUser;
    }

    public Task<AdminDashboardDto> GetAdminSummaryAsync(CancellationToken cancellationToken) =>
        _queries.GetAdminSummaryAsync(cancellationToken);

    public Task<EmployeeDashboardDto> GetEmployeeSummaryAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAccessException("User must be authenticated");

        return _queries.GetEmployeeSummaryAsync(_currentUser.UserId.Value, cancellationToken);
    }
}
