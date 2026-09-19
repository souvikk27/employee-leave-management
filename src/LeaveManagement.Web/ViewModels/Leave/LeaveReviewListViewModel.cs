using LeaveManagement.Application.DTOs.Leave;

namespace LeaveManagement.Web.ViewModels.Leave;

public sealed class LeaveReviewListViewModel
{
    public IReadOnlyList<AdminLeaveListItemDto> Items { get; set; } =
        Array.Empty<AdminLeaveListItemDto>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public string? StatusFilter { get; set; }
    public string? Search { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}
