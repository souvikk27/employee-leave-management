using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Tests.Domain;

public sealed class LeaveRequestTests
{
    [Fact]
    public void Create_WithFromDateAfterToDate_Throws()
    {
        var id = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var from = new DateOnly(2026, 01, 10);
        var to = new DateOnly(2026, 01, 05);

        Assert.Throws<ArgumentException>(() =>
            LeaveRequest.Create(id, createdBy, employeeId, from, to, "Reason")
        );
    }

    [Fact]
    public void Create_WithEmptyReason_Throws()
    {
        var id = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var from = new DateOnly(2026, 01, 01);
        var to = new DateOnly(2026, 01, 05);

        Assert.Throws<ArgumentException>(() =>
            LeaveRequest.Create(id, createdBy, employeeId, from, to, "   ")
        );
    }

    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var id = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var from = new DateOnly(2026, 01, 01);
        var to = new DateOnly(2026, 01, 05);

        var request = LeaveRequest.Create(id, createdBy, employeeId, from, to, "Vacation");

        Assert.Equal(LeaveStatus.Pending, request.Status);
        Assert.True(request.IsPending);
        Assert.False(request.IsTerminal);
    }

    [Fact]
    public void Approve_FromPending_Succeeds()
    {
        var request = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 01),
            new DateOnly(2026, 01, 05),
            "Reason"
        );

        var adminId = Guid.NewGuid();
        request.Approve(adminId);

        Assert.Equal(LeaveStatus.Approved, request.Status);
        Assert.True(request.IsTerminal);
        Assert.Equal(adminId, request.ReviewedBy);
        Assert.NotNull(request.ReviewedAt);
    }

    [Fact]
    public void Approve_FromNonPending_Throws()
    {
        var request = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 01),
            new DateOnly(2026, 01, 05),
            "Reason"
        );
        request.Approve(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => request.Approve(Guid.NewGuid()));
    }

    [Fact]
    public void Reject_FromPending_Succeeds()
    {
        var request = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 01),
            new DateOnly(2026, 01, 05),
            "Reason"
        );

        var adminId = Guid.NewGuid();
        request.Reject(adminId);

        Assert.Equal(LeaveStatus.Rejected, request.Status);
        Assert.True(request.IsTerminal);
    }

    [Fact]
    public void OverlapsWith_IgnoresRejected()
    {
        var a = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 01),
            new DateOnly(2026, 01, 10),
            "A"
        );
        var b = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 05),
            new DateOnly(2026, 01, 15),
            "B"
        );
        b.Reject(Guid.NewGuid());

        Assert.False(a.OverlapsWith(b));
    }

    [Fact]
    public void OverlapsWith_DetectsOverlap()
    {
        var a = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 01),
            new DateOnly(2026, 01, 10),
            "A"
        );
        var b = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 05),
            new DateOnly(2026, 01, 15),
            "B"
        );

        Assert.True(a.OverlapsWith(b));
        Assert.True(b.OverlapsWith(a));
    }

    [Fact]
    public void OverlapsWith_NoOverlap_WhenAdjacent()
    {
        var a = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 01),
            new DateOnly(2026, 01, 05),
            "A"
        );
        var b = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 06),
            new DateOnly(2026, 01, 10),
            "B"
        );

        Assert.False(a.OverlapsWith(b));
    }
}
