using LeaveManagement.Application.DTOs.Leave;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Services;
using LeaveManagement.Domain.Constants;
using LeaveManagement.Web.ViewModels.Leave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.Web.Controllers;

[Authorize(Roles = Roles.Employee)]
public sealed class LeavesController : Controller
{
    private readonly LeaveApplicationService _service;
    private readonly ILeaveQueries _queries;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<LeavesController> _logger;

    public LeavesController(
        LeaveApplicationService service,
        ILeaveQueries queries,
        ICurrentUserService currentUser,
        ILogger<LeavesController> logger
    )
    {
        _service = service;
        _queries = queries;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Apply()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return View(new ApplyLeaveViewModel { FromDate = today, ToDate = today });
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? status,
        string? search,
        int page = 1,
        CancellationToken ct = default
    )
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        if (page < 1)
            page = 1;
        var items = await _queries.ListOwnLeavesAsync(userId, status, search, page, 10, ct);
        var totalCount = await _queries.CountOwnLeavesAsync(userId, status, search, ct);
        var vm = new LeaveListViewModel
        {
            Items = items,
            Page = page,
            PageSize = 10,
            TotalCount = totalCount,
            StatusFilter = status,
            Search = search,
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyLeaveViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var dto = new ApplyLeaveDto
        {
            FromDate = vm.FromDate,
            ToDate = vm.ToDate,
            Reason = vm.Reason,
        };

        try
        {
            await _service.ApplyLeaveAsync(dto, ct);
            _logger.LogInformation(
                "Leave request submitted for {FromDate} to {ToDate}",
                dto.FromDate,
                dto.ToDate
            );
            TempData["Success"] = "Leave request submitted successfully.";
            return RedirectToAction(nameof(Apply));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Leave request rejected by validation: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Leave request rejected by business rule: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }
}
