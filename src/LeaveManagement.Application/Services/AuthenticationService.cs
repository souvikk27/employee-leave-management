using LeaveManagement.Application.DTOs;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserAuthenticationStore _store;
    private readonly IPasswordHasher _hasher;

    public AuthenticationService(IUserAuthenticationStore store, IPasswordHasher hasher)
    {
        _store = store;
        _hasher = hasher;
    }

    public async Task<AuthenticationResult?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var authUser = await _store.FindByEmailAsync(
            email.Trim().ToLowerInvariant(),
            cancellationToken
        );
        if (authUser is null)
            return null;

        var user = authUser.User;

        if (!user.IsActive)
            return null;

        if (!_hasher.Verify(password, user.PasswordHash))
            return null;

        var roles = authUser.Roles.Select(r => r.Name).ToList();

        if (roles.Contains("Employee"))
        {
            if (authUser.EmployeeIsActive != true)
                return null;
        }

        return new AuthenticationResult
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = roles,
        };
    }
}
