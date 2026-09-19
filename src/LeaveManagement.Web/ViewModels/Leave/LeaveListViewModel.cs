using LeaveManagement.Application.DTOs.Leave;

namespace LeaveManagement.Web.ViewModels.Leave;

public sealed class LeaveListViewModel
{
    public IReadOnlyList<LeaveListItemDto> Items { get; set; } = Array.Empty<LeaveListItemDto>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public string? StatusFilter { get; set; }
    public string? Search { get; set; }
}
