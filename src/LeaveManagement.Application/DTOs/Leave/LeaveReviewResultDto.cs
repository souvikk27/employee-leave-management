namespace LeaveManagement.Application.DTOs.Leave;

public sealed class LeaveReviewResultDto
{
    public Guid EmployeeUserId { get; set; }
    public DateTimeOffset ReviewedAt { get; set; }
    public string ReviewedByEmail { get; set; } = string.Empty;
}
