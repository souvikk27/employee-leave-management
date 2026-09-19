using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Web.ViewModels.Employee;

public sealed class EmployeeCreateViewModel
{
    [Required]
    [StringLength(256)]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
