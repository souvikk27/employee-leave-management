using Dapper;
using LeaveManagement.Application.DTOs.Leave;
using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Infrastructure.Persistence;

public sealed class LeaveQueries : ILeaveQueries
{
    private readonly ISqlConnectionFactory _factory;

    public LeaveQueries(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    private static int? NormalizeStatus(string? statusFilter) =>
        string.IsNullOrWhiteSpace(statusFilter)
            ? null
            : statusFilter.Trim().ToLowerInvariant() switch
            {
                "pending" => 1,
                "approved" => 2,
                "rejected" => 3,
                _ => null,
            };

    private static string? NormalizeSearch(string? search) =>
        string.IsNullOrWhiteSpace(search) ? null : search.Trim();

    public async Task<IReadOnlyList<LeaveListItemDto>> ListOwnLeavesAsync(
        Guid userId,
        string? statusFilter,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        int? statusValue = NormalizeStatus(statusFilter);
        var searchValue = NormalizeSearch(search);
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 10;

        await using var conn = await _factory.CreateOpenConnectionAsync(cancellationToken);
        var sql =
            @"
            SELECT
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
              AND (@statusValue IS NULL OR lr.Status = @statusValue)
              AND (@searchValue IS NULL OR lr.Reason LIKE '%' + @searchValue + '%')
            ORDER BY lr.CreatedAt DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
        ";
        var offset = (page - 1) * pageSize;
        var rows = await conn.QueryAsync<LeaveListItemDto>(
            sql,
            new
            {
                userId,
                statusValue,
                searchValue,
                offset,
                pageSize,
            }
        );
        return rows.AsList();
    }

    public async Task<IReadOnlyList<AdminLeaveListItemDto>> ListAllLeavesAsync(
        string? statusFilter,
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        int? statusValue = NormalizeStatus(statusFilter);
        var searchValue = NormalizeSearch(search);
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 10;

        await using var conn = await _factory.CreateOpenConnectionAsync(cancellationToken);
        var sql =
            @"
            SELECT
                lr.Id,
                e.Name AS EmployeeName,
                u.Email AS EmployeeEmail,
                lr.FromDate,
                lr.ToDate,
                lr.Reason,
                CASE lr.Status
                    WHEN 1 THEN 'Pending'
                    WHEN 2 THEN 'Approved'
                    WHEN 3 THEN 'Rejected'
                    ELSE CAST(lr.Status AS NVARCHAR(10))
                END AS Status,
                lr.CreatedAt,
                ru.Email AS ReviewedByEmail,
                lr.ReviewedAt
            FROM LeaveRequests lr
            JOIN Employees e ON e.Id = lr.EmployeeId
            JOIN Users u ON u.Id = e.UserId
            LEFT JOIN Users ru ON ru.Id = lr.ReviewedBy
            WHERE (@statusValue IS NULL OR lr.Status = @statusValue)
              AND (@searchValue IS NULL
                   OR lr.Reason LIKE '%' + @searchValue + '%'
                   OR e.Name LIKE '%' + @searchValue + '%'
                   OR u.Email LIKE '%' + @searchValue + '%')
              AND (@fromDate IS NULL OR lr.ToDate >= @fromDate)
              AND (@toDate IS NULL OR lr.FromDate <= @toDate)
            ORDER BY lr.CreatedAt DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
        ";
        var offset = (page - 1) * pageSize;
        var rows = await conn.QueryAsync<AdminLeaveListItemDto>(
            sql,
            new
            {
                statusValue,
                searchValue,
                fromDate,
                toDate,
                offset,
                pageSize,
            }
        );
        return rows.AsList();
    }

    public async Task<int> CountOwnLeavesAsync(
        Guid userId,
        string? statusFilter,
        string? search,
        CancellationToken cancellationToken
    )
    {
        int? statusValue = NormalizeStatus(statusFilter);
        var searchValue = NormalizeSearch(search);

        await using var conn = await _factory.CreateOpenConnectionAsync(cancellationToken);
        const string sql =
            @"
            SELECT COUNT(*)
            FROM LeaveRequests lr
            JOIN Employees e ON e.Id = lr.EmployeeId
            WHERE e.UserId = @userId
              AND (@statusValue IS NULL OR lr.Status = @statusValue)
              AND (@searchValue IS NULL OR lr.Reason LIKE '%' + @searchValue + '%');
        ";
        return await conn.ExecuteScalarAsync<int>(
            sql,
            new
            {
                userId,
                statusValue,
                searchValue,
            }
        );
    }

    public async Task<int> CountAllLeavesAsync(
        string? statusFilter,
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken
    )
    {
        int? statusValue = NormalizeStatus(statusFilter);
        var searchValue = NormalizeSearch(search);

        await using var conn = await _factory.CreateOpenConnectionAsync(cancellationToken);
        const string sql =
            @"
            SELECT COUNT(*)
            FROM LeaveRequests lr
            JOIN Employees e ON e.Id = lr.EmployeeId
            JOIN Users u ON u.Id = e.UserId
            WHERE (@statusValue IS NULL OR lr.Status = @statusValue)
              AND (@searchValue IS NULL
                   OR lr.Reason LIKE '%' + @searchValue + '%'
                   OR e.Name LIKE '%' + @searchValue + '%'
                   OR u.Email LIKE '%' + @searchValue + '%')
              AND (@fromDate IS NULL OR lr.ToDate >= @fromDate)
              AND (@toDate IS NULL OR lr.FromDate <= @toDate);
        ";
        return await conn.ExecuteScalarAsync<int>(
            sql,
            new
            {
                statusValue,
                searchValue,
                fromDate,
                toDate,
            }
        );
    }
}
