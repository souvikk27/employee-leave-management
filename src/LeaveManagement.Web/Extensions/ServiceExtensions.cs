using LeaveManagement.Application;
using LeaveManagement.Application.Services;
using LeaveManagement.Infrastructure;
using LeaveManagement.Web.Services;

namespace LeaveManagement.Web.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddWebCoreServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddControllersWithViews();
        services.AddHealthChecks();
        services
            .AddAuthentication("Cookies")
            .AddCookie(
                "Cookies",
                options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(14);
                }
            );
        services.AddAuthorization();
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
        // Friendly error page (RequestId only, no internals) for 4xx/5xx with empty bodies, e.g. 404s.
        app.UseStatusCodePagesWithReExecute("/Home/Error");
        app.UseAuthentication();
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
