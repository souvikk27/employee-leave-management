using LeaveManagement.Application.DTOs.Employee;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Constants;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Employees;

public sealed class EmployeeCommands : IEmployeeCommands
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public EmployeeCommands(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task CreateEmployeeAsync(
        EmployeeCreateDto dto,
        Guid performedBy,
        CancellationToken cancellationToken
    )
    {
        if (
            string.IsNullOrWhiteSpace(dto.Name)
            || string.IsNullOrWhiteSpace(dto.Email)
            || string.IsNullOrWhiteSpace(dto.Password)
        )
            throw new ArgumentException("Name, Email and Password are required");

        var email = dto.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == email, cancellationToken);
        if (exists)
            throw new InvalidOperationException("Email already in use");

        var role =
            await _db.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Employee, cancellationToken)
            ?? throw new InvalidOperationException("Employee role not found");

        var userId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var passwordHash = _hasher.Hash(dto.Password);
        var now = DateTimeOffset.UtcNow;

        var user = User.Create(userId, performedBy, email, passwordHash);
        var employee = new Employee(
            employeeId,
            now,
            now,
            performedBy,
            performedBy,
            dto.Name,
            userId,
            true
        );

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        _db.Users.Add(user);
        _db.Employees.Add(employee);
        var userRole = UserRole.Create(Guid.NewGuid(), performedBy, userId, role.Id);
        _db.UserRoles.Add(userRole);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Email already in use");
        }
        await tx.CommitAsync(cancellationToken);
    }

    public async Task UpdateEmployeeAsync(
        EmployeeUpdateDto dto,
        Guid performedBy,
        CancellationToken cancellationToken
    )
    {
        var employee =
            await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Employee not found");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");

        employee.UpdateDetails(dto.Name, performedBy);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateEmployeeAsync(
        Guid employeeId,
        Guid performedBy,
        CancellationToken cancellationToken
    )
    {
        var employee =
            await _db
                .Employees.Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Employee not found");

        if (!employee.IsActive)
            return;

        employee.Deactivate(performedBy);

        if (employee.User != null)
        {
            employee.User.Deactivate(performedBy);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivateEmployeeAsync(
        Guid employeeId,
        Guid performedBy,
        CancellationToken cancellationToken
    )
    {
        var employee =
            await _db
                .Employees.Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Employee not found");

        if (employee.IsActive)
            return;

        employee.Activate(performedBy);

        if (employee.User != null)
        {
            employee.User.Activate(performedBy);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
