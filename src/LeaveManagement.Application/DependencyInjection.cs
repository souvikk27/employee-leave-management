using Microsoft.Extensions.DependencyInjection;

namespace LeaveManagement.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<
            LeaveManagement.Application.Interfaces.IAuthenticationService,
            LeaveManagement.Application.Services.AuthenticationService
        >();
        return services;
    }
}
