using LeaveManagement.Application.DTOs.Leave;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Services;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Infrastructure.Leaves;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Tests.Leave;

public sealed class LeaveCommandsTests : IDisposable
{
    private static readonly DateTimeOffset FixedNow = new(2026, 9, 19, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = DateOnly.FromDateTime(FixedNow.UtcDateTime);

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public LeaveCommandsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = NewDb();
        db.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private AppDbContext NewDb() => new(_options);

    private sealed class FixedTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => FixedNow;
    }

    private sealed class FakeCurrentUser : ICurrentUserService
    {
        private readonly Guid? _userId;

        public FakeCurrentUser(Guid? userId) => _userId = userId;

        public Guid? UserId => _userId;

        public bool IsAuthenticated => _userId is not null;
    }

    private sealed class NoOpLeaveNotifier : ILeaveNotifier
    {
        public Task NotifyLeaveReviewedAsync(
            Guid employeeUserId,
            Guid leaveId,
            string status,
            DateTimeOffset reviewedAt,
            string reviewedByEmail,
            CancellationToken cancellationToken
        ) => Task.CompletedTask;
    }

    private static async Task<(Guid UserId, Guid EmployeeId)> SeedEmployeeAsync(
        AppDbContext db,
        string email,
        bool employeeActive = true,
        bool userActive = true
    )
    {
        var performedBy = Guid.NewGuid();
        var user = User.Create(Guid.NewGuid(), performedBy, email, "hash");
        if (!userActive)
            user.Deactivate(performedBy);
        var employee = new Employee(
            Guid.NewGuid(),
            FixedNow,
            FixedNow,
            performedBy,
            performedBy,
            "Test Employee",
            user.Id,
            employeeActive
        );
        db.Users.Add(user);
        db.Employees.Add(employee);
        await db.SaveChangesAsync();
        return (user.Id, employee.Id);
    }

    private static LeaveRequest NewLeave(
        Guid createdBy,
        Guid employeeId,
        DateOnly from,
        DateOnly to
    ) => LeaveRequest.Create(Guid.NewGuid(), createdBy, employeeId, from, to, "Reason", FixedNow);

    private static ApplyLeaveDto NewRequest(
        DateOnly from,
        DateOnly to,
        string reason = "Vacation"
    ) =>
        new()
        {
            FromDate = from,
            ToDate = to,
            Reason = reason,
        };

    [Fact]
    public async Task Apply_CreatesPendingLeave_ForCurrentUsersEmployee()
    {
        using var db = NewDb();
        var (userId, employeeId) = await SeedEmployeeAsync(db, "employee@test.com");
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await commands.ApplyLeaveAsync(
            userId,
            NewRequest(Today, Today.AddDays(2)),
            CancellationToken.None
        );

        var saved = await db.LeaveRequests.SingleAsync();
        Assert.Equal(LeaveStatus.Pending, saved.Status);
        Assert.Equal(employeeId, saved.EmployeeId);
        Assert.Equal(Today, saved.FromDate);
        Assert.Equal("Vacation", saved.Reason);
    }

    [Fact]
    public async Task Apply_SecondEmployee_SubmitsUnderOwnEmployee()
    {
        using var db = NewDb();
        var (_, employeeA) = await SeedEmployeeAsync(db, "a@test.com");
        var (userB, employeeB) = await SeedEmployeeAsync(db, "b@test.com");
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await commands.ApplyLeaveAsync(
            userB,
            NewRequest(Today, Today.AddDays(1)),
            CancellationToken.None
        );

        var saved = await db.LeaveRequests.SingleAsync();
        Assert.Equal(employeeB, saved.EmployeeId);
        Assert.NotEqual(employeeA, saved.EmployeeId);
    }

    [Fact]
    public async Task Apply_FromDateAfterToDate_ThrowsArgumentException()
    {
        using var db = NewDb();
        var (userId, _) = await SeedEmployeeAsync(db, "employee@test.com");
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            commands.ApplyLeaveAsync(
                userId,
                NewRequest(Today.AddDays(5), Today.AddDays(1)),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Apply_StartInPast_ThrowsArgumentException()
    {
        using var db = NewDb();
        var (userId, _) = await SeedEmployeeAsync(db, "employee@test.com");
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            commands.ApplyLeaveAsync(
                userId,
                NewRequest(Today.AddDays(-1), Today),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Apply_EmptyReason_ThrowsArgumentException()
    {
        using var db = NewDb();
        var (userId, _) = await SeedEmployeeAsync(db, "employee@test.com");
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            commands.ApplyLeaveAsync(
                userId,
                NewRequest(Today, Today.AddDays(1), "   "),
                CancellationToken.None
            )
        );
    }

    [Theory]
    [InlineData(LeaveStatus.Pending, true)]
    [InlineData(LeaveStatus.Approved, true)]
    [InlineData(LeaveStatus.Rejected, false)]
    public async Task Apply_ExistingBlockingStatuses_DeterminesOutcome(
        LeaveStatus existingStatus,
        bool shouldBlock
    )
    {
        using var db = NewDb();
        var (userId, employeeId) = await SeedEmployeeAsync(db, "employee@test.com");
        var adminId = Guid.NewGuid();
        var existing = NewLeave(userId, employeeId, Today, Today.AddDays(4));
        if (existingStatus == LeaveStatus.Approved)
            existing.Approve(adminId);
        else if (existingStatus == LeaveStatus.Rejected)
            existing.Reject(adminId);
        db.LeaveRequests.Add(existing);
        await db.SaveChangesAsync();
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        if (shouldBlock)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                commands.ApplyLeaveAsync(
                    userId,
                    NewRequest(Today.AddDays(2), Today.AddDays(6)),
                    CancellationToken.None
                )
            );
        }
        else
        {
            await commands.ApplyLeaveAsync(
                userId,
                NewRequest(Today.AddDays(2), Today.AddDays(6)),
                CancellationToken.None
            );
            Assert.Equal(2, await db.LeaveRequests.CountAsync());
        }
    }

    [Fact]
    public async Task Apply_BoundaryOverlap_IsBlocked()
    {
        using var db = NewDb();
        var (userId, employeeId) = await SeedEmployeeAsync(db, "employee@test.com");
        db.LeaveRequests.Add(NewLeave(userId, employeeId, Today, Today.AddDays(4)));
        await db.SaveChangesAsync();
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            commands.ApplyLeaveAsync(
                userId,
                NewRequest(Today.AddDays(4), Today.AddDays(6)),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Apply_AdjacentDates_AreAllowed()
    {
        using var db = NewDb();
        var (userId, employeeId) = await SeedEmployeeAsync(db, "employee@test.com");
        db.LeaveRequests.Add(NewLeave(userId, employeeId, Today, Today.AddDays(4)));
        await db.SaveChangesAsync();
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await commands.ApplyLeaveAsync(
            userId,
            NewRequest(Today.AddDays(5), Today.AddDays(7)),
            CancellationToken.None
        );

        Assert.Equal(2, await db.LeaveRequests.CountAsync());
    }

    [Fact]
    public async Task Apply_InactiveEmployee_ThrowsInvalidOperation()
    {
        using var db = NewDb();
        var (userId, _) = await SeedEmployeeAsync(db, "employee@test.com", employeeActive: false);
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            commands.ApplyLeaveAsync(
                userId,
                NewRequest(Today, Today.AddDays(1)),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Apply_InactiveUser_ThrowsInvalidOperation()
    {
        using var db = NewDb();
        var (userId, _) = await SeedEmployeeAsync(db, "employee@test.com", userActive: false);
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            commands.ApplyLeaveAsync(
                userId,
                NewRequest(Today, Today.AddDays(1)),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Apply_UnknownUser_ThrowsKeyNotFound()
    {
        using var db = NewDb();
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            commands.ApplyLeaveAsync(
                Guid.NewGuid(),
                NewRequest(Today, Today.AddDays(1)),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task ApplyService_Unauthenticated_ThrowsUnauthorized()
    {
        using var db = NewDb();
        var service = new LeaveApplicationService(
            new LeaveCommands(db, new FixedTimeProvider()),
            new FakeCurrentUser(null)
        );

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ApplyLeaveAsync(NewRequest(Today, Today.AddDays(1)), CancellationToken.None)
        );
    }

    [Fact]
    public async Task Approve_Pending_SetsApprovedWithAudit()
    {
        using var db = NewDb();
        var (userId, employeeId) = await SeedEmployeeAsync(db, "employee@test.com");
        var leave = NewLeave(userId, employeeId, Today, Today.AddDays(1));
        db.LeaveRequests.Add(leave);
        await db.SaveChangesAsync();
        var commands = new LeaveCommands(db, new FixedTimeProvider());
        var adminId = Guid.NewGuid();

        await commands.ApproveLeaveAsync(adminId, leave.Id, CancellationToken.None);

        var reviewed = await db.LeaveRequests.SingleAsync(l => l.Id == leave.Id);
        Assert.Equal(LeaveStatus.Approved, reviewed.Status);
        Assert.Equal(adminId, reviewed.ReviewedBy);
        Assert.NotNull(reviewed.ReviewedAt);
    }

    [Fact]
    public async Task Reject_Pending_SetsRejected()
    {
        using var db = NewDb();
        var (userId, employeeId) = await SeedEmployeeAsync(db, "employee@test.com");
        var leave = NewLeave(userId, employeeId, Today, Today.AddDays(1));
        db.LeaveRequests.Add(leave);
        await db.SaveChangesAsync();
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await commands.RejectLeaveAsync(Guid.NewGuid(), leave.Id, CancellationToken.None);

        Assert.Equal(
            LeaveStatus.Rejected,
            await db.LeaveRequests.Where(l => l.Id == leave.Id).Select(l => l.Status).SingleAsync()
        );
    }

    [Fact]
    public async Task Approve_AlreadyApproved_ThrowsInvalidOperation()
    {
        using var db = NewDb();
        var (userId, employeeId) = await SeedEmployeeAsync(db, "employee@test.com");
        var leave = NewLeave(userId, employeeId, Today, Today.AddDays(1));
        leave.Approve(Guid.NewGuid());
        db.LeaveRequests.Add(leave);
        await db.SaveChangesAsync();
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            commands.ApproveLeaveAsync(Guid.NewGuid(), leave.Id, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Approve_MissingRequest_ThrowsKeyNotFound()
    {
        using var db = NewDb();
        var commands = new LeaveCommands(db, new FixedTimeProvider());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            commands.ApproveLeaveAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None)
        );
    }

    [Fact]
    public async Task Approve_StaleVersion_SecondReviewerGetsFriendlyError()
    {
        using var db1 = NewDb();
        using var db2 = NewDb();
        var (userId, employeeId) = await SeedEmployeeAsync(db1, "employee@test.com");
        var adminId = Guid.NewGuid();
        var leave = NewLeave(userId, employeeId, Today, Today.AddDays(1));
        db1.LeaveRequests.Add(leave);
        await db1.SaveChangesAsync();

        _ = await db2.LeaveRequests.SingleAsync(l => l.Id == leave.Id);
        await new LeaveCommands(db1, new FixedTimeProvider()).ApproveLeaveAsync(
            adminId,
            leave.Id,
            CancellationToken.None
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new LeaveCommands(db2, new FixedTimeProvider()).ApproveLeaveAsync(
                adminId,
                leave.Id,
                CancellationToken.None
            )
        );
        Assert.Contains("already been reviewed", ex.Message);
    }

    [Fact]
    public async Task Approve_PersistsIncrementedVersionToken()
    {
        using var db = NewDb();
        var (userId, employeeId) = await SeedEmployeeAsync(db, "employee@test.com");
        var leave = NewLeave(userId, employeeId, Today, Today.AddDays(1));
        db.LeaveRequests.Add(leave);
        await db.SaveChangesAsync();

        await new LeaveCommands(db, new FixedTimeProvider()).ApproveLeaveAsync(
            Guid.NewGuid(),
            leave.Id,
            CancellationToken.None
        );

        using var fresh = NewDb();
        var version = await fresh
            .LeaveRequests.Where(l => l.Id == leave.Id)
            .Select(l => l.Version)
            .SingleAsync();
        Assert.Equal(2u, version);
    }

    [Fact]
    public async Task ReviewService_Unauthenticated_ThrowsUnauthorized()
    {
        using var db = NewDb();
        var service = new LeaveReviewService(
            new LeaveCommands(db, new FixedTimeProvider()),
            new FakeCurrentUser(null),
            new NoOpLeaveNotifier()
        );

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ApproveLeaveAsync(Guid.NewGuid(), CancellationToken.None)
        );
    }
}
