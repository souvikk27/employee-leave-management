using ClosedXML.Excel;
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
        DateOnly? fromDate,
        DateOnly? toDate,
        int page = 1,
        CancellationToken ct = default
    )
    {
        if (page < 1)
            page = 1;
        var items = await _queries.ListAllLeavesAsync(
            status,
            search,
            fromDate,
            toDate,
            page,
            10,
            ct
        );
        var totalCount = await _queries.CountAllLeavesAsync(status, search, fromDate, toDate, ct);
        var vm = new LeaveReviewListViewModel
        {
            Items = items,
            Page = page,
            PageSize = 10,
            TotalCount = totalCount,
            StatusFilter = status,
            Search = search,
            FromDate = fromDate,
            ToDate = toDate,
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Export(
        string? status,
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken ct = default
    )
    {
        var items = await _queries.ListAllLeavesAsync(
            status,
            search,
            fromDate,
            toDate,
            1,
            int.MaxValue,
            ct
        );

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Leave Requests");
        string[] headers =
        [
            "Employee",
            "Email",
            "From",
            "To",
            "Reason",
            "Status",
            "Submitted At",
            "Reviewed By",
            "Reviewed At",
        ];
        for (var col = 0; col < headers.Length; col++)
            sheet.Cell(1, col + 1).Value = headers[col];
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.EmployeeName;
            sheet.Cell(row, 2).Value = item.EmployeeEmail;
            sheet.Cell(row, 3).Value = item.FromDate.ToDateTime(TimeOnly.MinValue);
            sheet.Cell(row, 4).Value = item.ToDate.ToDateTime(TimeOnly.MinValue);
            sheet.Cell(row, 5).Value = item.Reason;
            sheet.Cell(row, 6).Value = item.Status;
            sheet.Cell(row, 7).Value = item.CreatedAt.LocalDateTime;
            sheet.Cell(row, 8).Value = item.ReviewedByEmail ?? string.Empty;
            if (item.ReviewedAt.HasValue)
                sheet.Cell(row, 9).Value = item.ReviewedAt.Value.LocalDateTime;
            row++;
        }
        sheet.Columns(3, 4).Style.DateFormat.Format = "yyyy-mm-dd";
        sheet.Columns(7, 9).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"leave-requests-{DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx"
        );
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
