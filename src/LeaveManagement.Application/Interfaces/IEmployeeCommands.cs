using LeaveManagement.Application.DTOs.Employee;

namespace LeaveManagement.Application.Interfaces;

public interface IEmployeeCommands
{
    Task CreateEmployeeAsync(
        EmployeeCreateDto dto,
        Guid performedBy,
        CancellationToken cancellationToken
    );
    Task UpdateEmployeeAsync(
        EmployeeUpdateDto dto,
        Guid performedBy,
        CancellationToken cancellationToken
    );
    Task DeactivateEmployeeAsync(
        Guid employeeId,
        Guid performedBy,
        CancellationToken cancellationToken
    );
}
