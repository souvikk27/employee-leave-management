using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Application.DTOs.Leave;

public sealed class ApplyLeaveDto
{
    [Required]
    public DateOnly FromDate { get; set; }

    [Required]
    public DateOnly ToDate { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}
