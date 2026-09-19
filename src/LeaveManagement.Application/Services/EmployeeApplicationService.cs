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
    ) => _queries.ListEmployeesAsync(search, statusFilter, page, pageSize, ct);

    public Task<int> CountEmployeesAsync(
        string? search,
        string? statusFilter,
        CancellationToken ct
    ) => _queries.CountEmployeesAsync(search, statusFilter, ct);

    public Task<EmployeeListItemDto?> GetEmployeeByIdAsync(Guid id, CancellationToken ct) =>
        _queries.GetEmployeeByIdAsync(id, ct);

    public async Task CreateEmployeeAsync(EmployeeCreateDto dto, CancellationToken ct)
    {
        var performedBy =
            _currentUser.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
        await _commands.CreateEmployeeAsync(dto, performedBy, ct);
    }

    public async Task UpdateEmployeeAsync(EmployeeUpdateDto dto, CancellationToken ct)
    {
        var performedBy =
            _currentUser.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
        await _commands.UpdateEmployeeAsync(dto, performedBy, ct);
    }

    public async Task DeactivateEmployeeAsync(Guid employeeId, CancellationToken ct)
    {
        var performedBy =
            _currentUser.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
        await _commands.DeactivateEmployeeAsync(employeeId, performedBy, ct);
    }
}
