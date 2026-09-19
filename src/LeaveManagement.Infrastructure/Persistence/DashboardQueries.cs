using Dapper;
using LeaveManagement.Application.DTOs.Dashboard;
using LeaveManagement.Application.DTOs.Leave;
using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Infrastructure.Persistence;

public sealed class DashboardQueries : IDashboardQueries
{
    private readonly ISqlConnectionFactory _factory;

    public DashboardQueries(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<AdminDashboardDto> GetAdminSummaryAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _factory.CreateOpenConnectionAsync(cancellationToken);
        const string sql =
            @"
            SELECT
                (SELECT COUNT(*) FROM Employees) AS TotalEmployees,
                (SELECT COUNT(*) FROM Employees WHERE IsActive = 1) AS ActiveEmployees,
                ISNULL(SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END), 0) AS PendingRequests,
                ISNULL(SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END), 0) AS ApprovedRequests,
                ISNULL(SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END), 0) AS RejectedRequests,
                COUNT(*) AS TotalRequests
            FROM LeaveRequests;
        ";
        return await conn.QuerySingleAsync<AdminDashboardDto>(sql);
    }

    public async Task<EmployeeDashboardDto> GetEmployeeSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        await using var conn = await _factory.CreateOpenConnectionAsync(cancellationToken);
        const string summarySql =
            @"
            SELECT
                ISNULL(SUM(CASE WHEN lr.Status = 1 THEN 1 ELSE 0 END), 0) AS PendingRequests,
                ISNULL(SUM(CASE WHEN lr.Status = 2 THEN 1 ELSE 0 END), 0) AS ApprovedRequests,
                ISNULL(SUM(CASE WHEN lr.Status = 3 THEN 1 ELSE 0 END), 0) AS RejectedRequests,
                COUNT(*) AS TotalRequests
            FROM LeaveRequests lr
            JOIN Employees e ON e.Id = lr.EmployeeId
            WHERE e.UserId = @userId;
        ";
        const string recentSql =
            @"
            SELECT TOP 5
                lr.Id,
                lr.FromDate,
                lr.ToDate,
                lr.Reason,
                CASE lr.Status
                    WHEN 1 THEN 'Pending'
                    WHEN 2 THEN 'Approved'
                    WHEN 3 THEN 'Rejected'
                    ELSE CAST(lr.Status AS NVARCHAR(10))
                END AS Status,
                lr.CreatedAt
            FROM LeaveRequests lr
            JOIN Employees e ON e.Id = lr.EmployeeId
            WHERE e.UserId = @userId
            ORDER BY lr.CreatedAt DESC;
        ";
        var summary = await conn.QuerySingleAsync<EmployeeDashboardDto>(summarySql, new { userId });
        var recent = await conn.QueryAsync<LeaveListItemDto>(recentSql, new { userId });
        summary.RecentLeaves = recent.AsList();
        return summary;
    }
}
