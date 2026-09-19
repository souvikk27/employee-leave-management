using Dapper;
using LeaveManagement.Application.DTOs.Employee;
using LeaveManagement.Application.Interfaces;
using Microsoft.Data.SqlClient;

namespace LeaveManagement.Infrastructure.Persistence;

public sealed class EmployeeQueries : IEmployeeQueries
{
    private readonly ISqlConnectionFactory _factory;

    public EmployeeQueries(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    private static string? NormalizeSearch(string? search) =>
        string.IsNullOrWhiteSpace(search) ? null : search.Trim();

    private static bool? NormalizeStatus(string? statusFilter) =>
        string.IsNullOrWhiteSpace(statusFilter)
            ? null
            : statusFilter.Trim().ToLowerInvariant() switch
            {
                "active" => true,
                "inactive" => false,
                _ => null,
            };

    public async Task<IReadOnlyList<EmployeeListItemDto>> ListEmployeesAsync(
        string? search,
        string? statusFilter,
        Guid excludeUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        var searchValue = NormalizeSearch(search);
        bool? isActive = NormalizeStatus(statusFilter);
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 20;

        await using var conn = await _factory.CreateOpenConnectionAsync(cancellationToken);
        var sql =
            @"
            SELECT e.Id, e.Name, u.Email, e.IsActive,
                (SELECT COUNT(*) FROM LeaveRequests lr WHERE lr.EmployeeId = e.Id) AS LeaveCount
            FROM Employees e
            JOIN Users u ON u.Id = e.UserId
            WHERE e.UserId <> @excludeUserId
              AND (@searchValue IS NULL OR e.Name LIKE '%' + @searchValue + '%' OR u.Email LIKE '%' + @searchValue + '%')
              AND (@isActive IS NULL OR e.IsActive = @isActive)
            ORDER BY e.CreatedAt DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
        ";
        var offset = (page - 1) * pageSize;
        var results = await conn.QueryAsync<EmployeeListItemDto>(
            sql,
            new
            {
                searchValue,
                isActive,
                excludeUserId,
                offset,
                pageSize,
            }
        );
        return results.AsList();
    }

    public async Task<int> CountEmployeesAsync(
        string? search,
        string? statusFilter,
        Guid excludeUserId,
        CancellationToken cancellationToken
    )
    {
        var searchValue = NormalizeSearch(search);
        bool? isActive = NormalizeStatus(statusFilter);

        await using var conn = await _factory.CreateOpenConnectionAsync(cancellationToken);
        const string sql =
            @"
            SELECT COUNT(*)
            FROM Employees e
            JOIN Users u ON u.Id = e.UserId
            WHERE e.UserId <> @excludeUserId
              AND (@searchValue IS NULL OR e.Name LIKE '%' + @searchValue + '%' OR u.Email LIKE '%' + @searchValue + '%')
              AND (@isActive IS NULL OR e.IsActive = @isActive);
        ";
        return await conn.ExecuteScalarAsync<int>(
            sql,
            new
            {
                searchValue,
                isActive,
                excludeUserId,
            }
        );
    }

    public async Task<EmployeeListItemDto?> GetEmployeeByIdAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        await using var conn = await _factory.CreateOpenConnectionAsync(cancellationToken);
        var sql =
            @"
            SELECT e.Id, e.Name, u.Email, e.IsActive,
                (SELECT COUNT(*) FROM LeaveRequests lr WHERE lr.EmployeeId = e.Id) AS LeaveCount
            FROM Employees e
            JOIN Users u ON u.Id = e.UserId
            WHERE e.Id = @id;
        ";
        var result = await conn.QuerySingleOrDefaultAsync<EmployeeListItemDto>(sql, new { id });
        return result;
    }
}
