using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Tests.Domain;

public sealed class EmployeeTests
{
    private static void SetUser(Employee employee, User user)
    {
        var prop = typeof(Employee).GetProperty(
            "User",
            System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public
        );
        prop?.SetValue(employee, user);
    }

    [Fact]
    public void CanSubmitLeave_ActiveEmployeeAndActiveUser_ReturnsTrue()
    {
        var user = User.Create(Guid.NewGuid(), Guid.NewGuid(), "email@test.com", "hash");
        var now = DateTimeOffset.UtcNow;
        var createdBy = Guid.NewGuid();
        var employee = new Employee(
            Guid.NewGuid(),
            now,
            now,
            createdBy,
            createdBy,
            "Name",
            user.Id,
            true
        );
        SetUser(employee, user);

        Assert.True(employee.CanSubmitLeave());
    }

    [Fact]
    public void CanSubmitLeave_InactiveEmployee_ReturnsFalse()
    {
        var user = User.Create(Guid.NewGuid(), Guid.NewGuid(), "email@test.com", "hash");
        var now = DateTimeOffset.UtcNow;
        var createdBy = Guid.NewGuid();
        var employee = new Employee(
            Guid.NewGuid(),
            now,
            now,
            createdBy,
            createdBy,
            "Name",
            user.Id,
            true
        );
        SetUser(employee, user);
        employee.Deactivate(Guid.NewGuid());

        Assert.False(employee.CanSubmitLeave());
    }

    [Fact]
    public void CanSubmitLeave_InactiveUser_ReturnsFalse()
    {
        var user = User.Create(Guid.NewGuid(), Guid.NewGuid(), "email@test.com", "hash");
        user.Deactivate(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;
        var createdBy = Guid.NewGuid();
        var employee = new Employee(
            Guid.NewGuid(),
            now,
            now,
            createdBy,
            createdBy,
            "Name",
            user.Id,
            true
        );
        SetUser(employee, user);

        Assert.False(employee.CanSubmitLeave());
    }
}
