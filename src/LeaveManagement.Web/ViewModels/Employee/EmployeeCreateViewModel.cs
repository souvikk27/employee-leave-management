using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Web.ViewModels.Employee;

public sealed class EmployeeCreateViewModel
{
    [Required]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
