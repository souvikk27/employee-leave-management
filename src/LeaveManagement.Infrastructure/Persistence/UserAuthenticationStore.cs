using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Persistence;

public sealed class EfUserAuthenticationStore : IUserAuthenticationStore
{
    private readonly AppDbContext _db;

    public EfUserAuthenticationStore(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AuthenticationUser?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken
    )
    {
        var user = await _db
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);

        if (user is null)
            return null;

        var userRoles = await _db
            .UserRoles.AsNoTracking()
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => new { ur.RoleId })
            .ToListAsync(cancellationToken);

        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

        var roles = await _db
            .Roles.AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        bool? employeeIsActive = null;
        if (roles.Any(r => r.Name == "Employee"))
        {
            var employee = await _db
                .Employees.AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == user.Id, cancellationToken);
            employeeIsActive = employee?.IsActive == true;
        }

        return new AuthenticationUser
        {
            User = user,
            Roles = roles,
            EmployeeIsActive = employeeIsActive,
        };
    }
}
