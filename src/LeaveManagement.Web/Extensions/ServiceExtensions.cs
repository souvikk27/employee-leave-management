using LeaveManagement.Application;
using LeaveManagement.Infrastructure;

namespace LeaveManagement.Web.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddWebCoreServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddControllersWithViews();
        services.AddHealthChecks();
        services.AddApplicationServices(configuration);
        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }

    public static WebApplication UseAppPipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapHealthChecks("/health");

        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        return app;
    }

    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "CorsPolicy",
                builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );
        });
    }
}
