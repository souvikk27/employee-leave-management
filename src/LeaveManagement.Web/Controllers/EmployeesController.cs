using LeaveManagement.Application.DTOs.Employee;
using LeaveManagement.Application.Services;
using LeaveManagement.Domain.Constants;
using LeaveManagement.Web.ViewModels.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public sealed class EmployeesController : Controller
{
    private readonly EmployeeApplicationService _service;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        EmployeeApplicationService service,
        ILogger<EmployeesController> logger
    )
    {
        _service = service;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search, string? status, int page = 1)
    {
        const int pageSize = 20;
        if (page < 1)
            page = 1;
        var items = await _service.ListEmployeesAsync(
            search,
            status,
            page,
            pageSize,
            HttpContext.RequestAborted
        );
        var totalCount = await _service.CountEmployeesAsync(
            search,
            status,
            HttpContext.RequestAborted
        );
        var model = items
            .Select(i => new EmployeeListViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Email = i.Email,
                IsActive = i.IsActive,
                LeaveCount = i.LeaveCount,
            })
            .ToList();
        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        return View(model);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeCreateViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);
        try
        {
            var dto = new EmployeeCreateDto
            {
                Name = vm.Name,
                Email = vm.Email,
                Password = vm.Password,
            };
            await _service.CreateEmployeeAsync(dto, HttpContext.RequestAborted);
            _logger.LogInformation("Employee created");
            TempData["Success"] = "Employee created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Employee creation rejected: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var item = await _service.GetEmployeeByIdAsync(id, HttpContext.RequestAborted);
        if (item is null)
            return NotFound();
        var vm = new EmployeeEditViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Email = item.Email,
            IsActive = item.IsActive,
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeEditViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);
        try
        {
            var dto = new EmployeeUpdateDto { Id = vm.Id, Name = vm.Name };
            await _service.UpdateEmployeeAsync(dto, HttpContext.RequestAborted);
            _logger.LogInformation("Employee {EmployeeId} updated", vm.Id);
            TempData["Success"] = "Employee updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _service.DeactivateEmployeeAsync(id, HttpContext.RequestAborted);
        _logger.LogInformation("Employee {EmployeeId} deactivated", id);
        TempData["Success"] = "Employee deactivated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
