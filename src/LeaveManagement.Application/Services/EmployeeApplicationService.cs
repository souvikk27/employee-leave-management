using LeaveManagement.Application.DTOs.Employee;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Services;

namespace LeaveManagement.Application.Services;

public sealed class EmployeeApplicationService
{
    private readonly IEmployeeCommands _commands;
    private readonly IEmployeeQueries _queries;
    private readonly ICurrentUserService _currentUser;

    public EmployeeApplicationService(
        IEmployeeCommands commands,
        IEmployeeQueries queries,
        ICurrentUserService currentUser
    )
    {
        _commands = commands;
        _queries = queries;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<EmployeeListItemDto>> ListEmployeesAsync(
        string? search,
        string? statusFilter,
        int page,
        int pageSize,
        CancellationToken ct
    ) =>
        _queries.ListEmployeesAsync(
            search,
            statusFilter,
            RequireCurrentUserId(),
            page,
            pageSize,
            ct
        );

    public Task<int> CountEmployeesAsync(
        string? search,
        string? statusFilter,
        CancellationToken ct
    ) => _queries.CountEmployeesAsync(search, statusFilter, RequireCurrentUserId(), ct);

    private Guid RequireCurrentUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

    public Task<EmployeeListItemDto?> GetEmployeeByIdAsync(Guid id, CancellationToken ct) =>
        _queries.GetEmployeeByIdAsync(id, ct);

    public Task CreateEmployeeAsync(EmployeeCreateDto dto, CancellationToken ct) =>
        _commands.CreateEmployeeAsync(dto, RequireCurrentUserId(), ct);

    public Task UpdateEmployeeAsync(EmployeeUpdateDto dto, CancellationToken ct) =>
        _commands.UpdateEmployeeAsync(dto, RequireCurrentUserId(), ct);

    public Task DeactivateEmployeeAsync(Guid employeeId, CancellationToken ct) =>
        _commands.DeactivateEmployeeAsync(employeeId, RequireCurrentUserId(), ct);

    public Task ActivateEmployeeAsync(Guid employeeId, CancellationToken ct) =>
        _commands.ActivateEmployeeAsync(employeeId, RequireCurrentUserId(), ct);
}
