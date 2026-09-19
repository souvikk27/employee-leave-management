using System.Security.Claims;
using LeaveManagement.Application;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Services;
using LeaveManagement.Infrastructure;
using LeaveManagement.Infrastructure.Persistence;
using LeaveManagement.Web.Hubs;
using LeaveManagement.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

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
        services.AddScoped<ILeaveNotifier, SignalRLeaveNotifier>();
        services.AddSignalR();

        services.AddControllersWithViews();
        services.AddHealthChecks();
        services
            .AddAuthentication("Cookies")
            .AddCookie(
                "Cookies",
                options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(14);
                    options.Events.OnRedirectToAccessDenied = ctx =>
                    {
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    };
                    options.Events.OnValidatePrincipal = async ctx =>
                    {
                        var idClaim = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (!Guid.TryParse(idClaim, out var userId))
                            return;
                        var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                        var active = await db.Users.AnyAsync(
                            u => u.Id == userId && u.IsActive,
                            ctx.HttpContext.RequestAborted
                        );
                        if (!active)
                        {
                            ctx.RejectPrincipal();
                            await ctx.HttpContext.SignOutAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme
                            );
                        }
                    };
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
        app.UseStatusCodePagesWithReExecute("/Home/Error");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapHealthChecks("/health");
        app.MapHub<LeaveNotificationsHub>("/hubs/leave-notifications");

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
