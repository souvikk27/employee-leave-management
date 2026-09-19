namespace LeaveManagement.Application.DTOs.Leave;

public sealed class LeaveListItemDto
{
    public Guid Id { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
