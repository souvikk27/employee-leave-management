using BCrypt.Net;
using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Infrastructure.Persistence;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
