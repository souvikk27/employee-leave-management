using LeaveManagement.Application.Interfaces;
using LeaveManagement.Infrastructure.Common;
using LeaveManagement.Infrastructure.Dashboards;
using LeaveManagement.Infrastructure.Employees;
using LeaveManagement.Infrastructure.Identity;
using LeaveManagement.Infrastructure.Leaves;
using LeaveManagement.Infrastructure.Persistence;
using LeaveManagement.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveManagement.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<AuditSaveChangesInterceptor>();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection connection string is missing. Copy "
                    + "src/LeaveManagement.Web/appsettings.example.json to appsettings.json, "
                    + "or provide it via user secrets "
                    + "(\"dotnet user-secrets set \\\"ConnectionStrings:DefaultConnection\\\" \\\"<value>\\\"\") "
                    + "or the ConnectionStrings__DefaultConnection environment variable. See README.md."
            );

        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(
            connectionString
        ));
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<
            LeaveManagement.Application.Interfaces.IPasswordHasher,
            BCryptPasswordHasher
        >();
        services.AddScoped<
            LeaveManagement.Application.Interfaces.IUserAuthenticationStore,
            EfUserAuthenticationStore
        >();
        services.AddScoped<
            LeaveManagement.Application.Interfaces.IEmployeeCommands,
            EmployeeCommands
        >();
        services.AddScoped<
            LeaveManagement.Application.Interfaces.IEmployeeQueries,
            EmployeeQueries
        >();
        services.AddScoped<LeaveManagement.Application.Interfaces.ILeaveCommands, LeaveCommands>();
        services.AddScoped<LeaveManagement.Application.Interfaces.ILeaveQueries, LeaveQueries>();
        services.AddScoped<
            LeaveManagement.Application.Interfaces.IDashboardQueries,
            DashboardQueries
        >();

        services.AddDbContext<AppDbContext>(
            (sp, options) =>
            {
                var interceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
                options.UseSqlServer(connectionString).AddInterceptors(interceptor);
            }
        );

        return services;
    }
}
