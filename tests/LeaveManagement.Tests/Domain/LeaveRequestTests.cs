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
            LeaveRequest.Create(
                id,
                createdBy,
                employeeId,
                from,
                to,
                "Reason",
                DateTimeOffset.UtcNow
            )
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
            LeaveRequest.Create(id, createdBy, employeeId, from, to, "   ", DateTimeOffset.UtcNow)
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

        var request = LeaveRequest.Create(
            id,
            createdBy,
            employeeId,
            from,
            to,
            "Vacation",
            DateTimeOffset.UtcNow
        );

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
            "Reason",
            DateTimeOffset.UtcNow
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
            "Reason",
            DateTimeOffset.UtcNow
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
            "Reason",
            DateTimeOffset.UtcNow
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
            "A",
            DateTimeOffset.UtcNow
        );
        var b = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 05),
            new DateOnly(2026, 01, 15),
            "B",
            DateTimeOffset.UtcNow
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
            "A",
            DateTimeOffset.UtcNow
        );
        var b = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 05),
            new DateOnly(2026, 01, 15),
            "B",
            DateTimeOffset.UtcNow
        );

        Assert.True(a.OverlapsWith(b));
        Assert.True(b.OverlapsWith(a));
    }

    [Fact]
    public void OverlapsWith_SameDayOverlap_DetectsOverlap()
    {
        var multi = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 01),
            new DateOnly(2026, 01, 10),
            "A",
            DateTimeOffset.UtcNow
        );
        var single = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 05),
            new DateOnly(2026, 01, 05),
            "B",
            DateTimeOffset.UtcNow
        );

        Assert.True(multi.OverlapsWith(single));
        Assert.True(single.OverlapsWith(multi));
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
            "A",
            DateTimeOffset.UtcNow
        );
        var b = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 01, 06),
            new DateOnly(2026, 01, 10),
            "B",
            DateTimeOffset.UtcNow
        );

        Assert.False(a.OverlapsWith(b));
    }
}
