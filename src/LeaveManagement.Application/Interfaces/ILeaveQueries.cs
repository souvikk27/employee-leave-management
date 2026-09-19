using LeaveManagement.Application.DTOs.Leave;

namespace LeaveManagement.Application.Interfaces;

public interface ILeaveQueries
{
    Task<IReadOnlyList<LeaveListItemDto>> ListOwnLeavesAsync(
        Guid userId,
        string? statusFilter,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );
    Task<IReadOnlyList<AdminLeaveListItemDto>> ListAllLeavesAsync(
        string? statusFilter,
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );
    Task<int> CountOwnLeavesAsync(
        Guid userId,
        string? statusFilter,
        string? search,
        CancellationToken cancellationToken
    );
    Task<int> CountAllLeavesAsync(
        string? statusFilter,
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken
    );
}
