using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Infrastructure.Persistence;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
