using LeaveManagement.Application.Interfaces;
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
                "DefaultConnection connection string is missing."
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
