using System.Data.Common;

namespace LeaveManagement.Application.Interfaces;

public interface ISqlConnectionFactory
{
    DbConnection CreateConnection();
    ValueTask<DbConnection> CreateOpenConnectionAsync(
        CancellationToken cancellationToken = default
    );
}
