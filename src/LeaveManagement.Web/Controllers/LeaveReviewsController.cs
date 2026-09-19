using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Services;
using LeaveManagement.Domain.Constants;
using LeaveManagement.Web.ViewModels.Leave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public sealed class LeaveReviewsController : Controller
{
    private readonly LeaveReviewService _service;
    private readonly ILeaveQueries _queries;
    private readonly ILogger<LeaveReviewsController> _logger;

    public LeaveReviewsController(
        LeaveReviewService service,
        ILeaveQueries queries,
        ILogger<LeaveReviewsController> logger
    )
    {
        _service = service;
        _queries = queries;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? status,
        string? search,
        int page = 1,
        CancellationToken ct = default
    )
    {
        if (page < 1)
            page = 1;
        var items = await _queries.ListAllLeavesAsync(status, search, page, 10, ct);
        var totalCount = await _queries.CountAllLeavesAsync(status, search, ct);
        var vm = new LeaveReviewListViewModel
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
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.ApproveLeaveAsync(id, ct);
            _logger.LogInformation("Leave request {LeaveId} approved", id);
            TempData["Success"] = "Leave request approved.";
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Approval attempted on missing leave request {LeaveId}", id);
            TempData["Error"] = "Leave request not found.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Approval rejected for leave request {LeaveId}: {Message}",
                id,
                ex.Message
            );
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.RejectLeaveAsync(id, ct);
            _logger.LogInformation("Leave request {LeaveId} rejected", id);
            TempData["Success"] = "Leave request rejected.";
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Rejection attempted on missing leave request {LeaveId}", id);
            TempData["Error"] = "Leave request not found.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Reject failed for leave request {LeaveId}: {Message}",
                id,
                ex.Message
            );
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
