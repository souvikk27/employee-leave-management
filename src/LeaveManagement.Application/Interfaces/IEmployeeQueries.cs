using LeaveManagement.Application.DTOs.Employee;

namespace LeaveManagement.Application.Interfaces;

public interface IEmployeeQueries
{
    Task<IReadOnlyList<EmployeeListItemDto>> ListEmployeesAsync(
        string? search,
        string? statusFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );
    Task<int> CountEmployeesAsync(
        string? search,
        string? statusFilter,
        CancellationToken cancellationToken
    );
    Task<EmployeeListItemDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken);
}
