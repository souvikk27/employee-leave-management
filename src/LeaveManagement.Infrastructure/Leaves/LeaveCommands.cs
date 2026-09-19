using LeaveManagement.Application.DTOs.Leave;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Leaves;

public sealed class LeaveCommands : ILeaveCommands
{
    private readonly AppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LeaveCommands(AppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task ApplyLeaveAsync(
        Guid userId,
        ApplyLeaveDto dto,
        CancellationToken cancellationToken
    )
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow.UtcDateTime.Date);
        if (dto.FromDate < today)
            throw new ArgumentException("Leave cannot start in the past");

        if (dto.FromDate > dto.ToDate)
            throw new ArgumentException("FromDate must be less than or equal to ToDate");

        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Reason is required");

        var employee =
            await _db
                .Employees.Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Employee not found for current user");

        if (!employee.CanSubmitLeave())
            throw new InvalidOperationException("Only active employees can submit leave");

        var now = _dateTimeProvider.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            cancellationToken
        );

        var hasOverlap = await _db.LeaveRequests.AnyAsync(
            l =>
                l.EmployeeId == employee.Id
                && l.Status != LeaveStatus.Rejected
                && l.FromDate <= dto.ToDate
                && l.ToDate >= dto.FromDate,
            cancellationToken
        );

        if (hasOverlap)
            throw new InvalidOperationException(
                "Requested leave overlaps with an existing pending or approved leave"
            );

        var leaveRequest = LeaveRequest.Create(
            Guid.NewGuid(),
            userId,
            employee.Id,
            dto.FromDate,
            dto.ToDate,
            dto.Reason.Trim(),
            now
        );

        _db.LeaveRequests.Add(leaveRequest);
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    public Task<LeaveReviewResultDto> ApproveLeaveAsync(
        Guid adminUserId,
        Guid leaveId,
        CancellationToken cancellationToken
    ) => ReviewLeaveAsync(adminUserId, leaveId, approve: true, cancellationToken);

    public Task<LeaveReviewResultDto> RejectLeaveAsync(
        Guid adminUserId,
        Guid leaveId,
        CancellationToken cancellationToken
    ) => ReviewLeaveAsync(adminUserId, leaveId, approve: false, cancellationToken);

    private async Task<LeaveReviewResultDto> ReviewLeaveAsync(
        Guid adminUserId,
        Guid leaveId,
        bool approve,
        CancellationToken cancellationToken
    )
    {
        var leaveRequest =
            await _db
                .LeaveRequests.Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == leaveId, cancellationToken)
            ?? throw new KeyNotFoundException("Leave request not found");

        if (approve)
            leaveRequest.Approve(adminUserId);
        else
            leaveRequest.Reject(adminUserId);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "This leave request has already been reviewed by another admin"
            );
        }

        var adminEmail = await _db
            .Users.Where(u => u.Id == adminUserId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        return new LeaveReviewResultDto
        {
            EmployeeUserId = leaveRequest.Employee!.UserId,
            ReviewedAt = leaveRequest.ReviewedAt!.Value,
            ReviewedByEmail = adminEmail ?? string.Empty,
        };
    }
}
