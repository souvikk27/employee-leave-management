using System.Data.Common;
using LeaveManagement.Application.Interfaces;
using Microsoft.Data.SqlClient;

namespace LeaveManagement.Infrastructure.Common;

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async ValueTask<DbConnection> CreateOpenConnectionAsync(
        CancellationToken cancellationToken = default
    )
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
