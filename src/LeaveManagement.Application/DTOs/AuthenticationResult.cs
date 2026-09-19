namespace LeaveManagement.Application.DTOs;

public sealed record AuthenticationResult
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
