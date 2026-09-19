namespace LeaveManagement.Web.ViewModels.Employee;

public sealed class EmployeeListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int LeaveCount { get; set; }
}
