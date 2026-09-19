using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Web.ViewModels.Leave;

public sealed class ApplyLeaveViewModel
{
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly FromDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly ToDate { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}
