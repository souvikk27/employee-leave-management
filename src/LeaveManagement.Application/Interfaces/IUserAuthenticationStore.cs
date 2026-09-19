using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Interfaces;

public sealed class AuthenticationUser
{
    public User User { get; init; } = default!;
    public IReadOnlyList<Role> Roles { get; init; } = Array.Empty<Role>();
    public bool? EmployeeIsActive { get; init; }
}

public interface IUserAuthenticationStore
{
    Task<AuthenticationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}
