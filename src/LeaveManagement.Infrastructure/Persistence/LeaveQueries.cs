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

    // LeaveStatus is persisted as int (Pending = 1, Approved = 2, Rejected = 3).
    // Normalize UI strings to the stored int; empty/unknown means "no filter".
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
        // Project the stored int back to its display name so the DTO/view receive "Pending"/etc.
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
                lr.CreatedAt
            FROM LeaveRequests lr
            JOIN Employees e ON e.Id = lr.EmployeeId
            JOIN Users u ON u.Id = e.UserId
            WHERE (@statusValue IS NULL OR lr.Status = @statusValue)
              AND (@searchValue IS NULL
                   OR lr.Reason LIKE '%' + @searchValue + '%'
                   OR e.Name LIKE '%' + @searchValue + '%'
                   OR u.Email LIKE '%' + @searchValue + '%')
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
                   OR u.Email LIKE '%' + @searchValue + '%');
        ";
        return await conn.ExecuteScalarAsync<int>(sql, new { statusValue, searchValue });
    }
}
