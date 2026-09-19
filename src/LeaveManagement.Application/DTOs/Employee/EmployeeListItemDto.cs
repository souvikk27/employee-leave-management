namespace LeaveManagement.Application.DTOs.Employee;

public sealed class EmployeeListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int LeaveCount { get; set; }
}
