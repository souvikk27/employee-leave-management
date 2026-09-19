using System.Diagnostics;
using LeaveManagement.Application.Services;
using LeaveManagement.Domain.Constants;
using LeaveManagement.Web.Models;
using LeaveManagement.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.Web.Controllers;

public class HomeController : Controller
{
    private readonly DashboardApplicationService _dashboards;
    private readonly ILogger<HomeController> _logger;

    public HomeController(DashboardApplicationService dashboards, ILogger<HomeController> logger)
    {
        _dashboards = dashboards;
        _logger = logger;
    }

    [Authorize]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (User.IsInRole(Roles.Admin))
        {
            var summary = await _dashboards.GetAdminSummaryAsync(ct);
            return View(
                "AdminDashboard",
                new AdminDashboardViewModel
                {
                    TotalEmployees = summary.TotalEmployees,
                    ActiveEmployees = summary.ActiveEmployees,
                    PendingRequests = summary.PendingRequests,
                    ApprovedRequests = summary.ApprovedRequests,
                    RejectedRequests = summary.RejectedRequests,
                    TotalRequests = summary.TotalRequests,
                }
            );
        }

        var own = await _dashboards.GetEmployeeSummaryAsync(ct);
        return View(
            "EmployeeDashboard",
            new EmployeeDashboardViewModel
            {
                PendingRequests = own.PendingRequests,
                ApprovedRequests = own.ApprovedRequests,
                RejectedRequests = own.RejectedRequests,
                TotalRequests = own.TotalRequests,
                RecentLeaves = own.RecentLeaves,
            }
        );
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (feature?.Error is not null)
            _logger.LogError(feature.Error, "Unhandled exception on {Path}", feature.Path);

        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
