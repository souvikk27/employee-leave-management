using LeaveManagement.Application.DTOs.Leave;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Persistence;

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

    public Task ApproveLeaveAsync(
        Guid adminUserId,
        Guid leaveId,
        CancellationToken cancellationToken
    ) => ReviewLeaveAsync(adminUserId, leaveId, approve: true, cancellationToken);

    public Task RejectLeaveAsync(
        Guid adminUserId,
        Guid leaveId,
        CancellationToken cancellationToken
    ) => ReviewLeaveAsync(adminUserId, leaveId, approve: false, cancellationToken);

    private async Task ReviewLeaveAsync(
        Guid adminUserId,
        Guid leaveId,
        bool approve,
        CancellationToken cancellationToken
    )
    {
        var leaveRequest =
            await _db.LeaveRequests.FirstOrDefaultAsync(l => l.Id == leaveId, cancellationToken)
            ?? throw new KeyNotFoundException("Leave request not found");

        // Domain enforces Pending -> Approved/Rejected; non-pending throws InvalidOperationException.
        if (approve)
            leaveRequest.Approve(adminUserId);
        else
            leaveRequest.Reject(adminUserId);

        // Single-row update is atomic via SaveChanges. The Version concurrency token
        // guards against two admins reviewing the same Pending request concurrently:
        // the loser gets DbUpdateConcurrencyException, translated below.
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
    }
}
