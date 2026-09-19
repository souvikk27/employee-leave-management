using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Web.ViewModels.Employee;

public sealed class EmployeeEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(256)]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
