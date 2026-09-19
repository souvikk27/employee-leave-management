namespace LeaveManagement.Application.DTOs.Employee;

public sealed class EmployeeUpdateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
