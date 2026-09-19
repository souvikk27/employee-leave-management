using LeaveManagement.Application.DTOs;

namespace LeaveManagement.Application.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResult?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken
    );
}
