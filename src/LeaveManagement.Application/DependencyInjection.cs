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
        services.AddScoped<
            LeaveManagement.Application.Services.EmployeeApplicationService,
            LeaveManagement.Application.Services.EmployeeApplicationService
        >();
        services.AddScoped<
            LeaveManagement.Application.Services.LeaveApplicationService,
            LeaveManagement.Application.Services.LeaveApplicationService
        >();
        services.AddScoped<
            LeaveManagement.Application.Services.LeaveReviewService,
            LeaveManagement.Application.Services.LeaveReviewService
        >();
        services.AddScoped<
            LeaveManagement.Application.Services.DashboardApplicationService,
            LeaveManagement.Application.Services.DashboardApplicationService
        >();
        return services;
    }
}
