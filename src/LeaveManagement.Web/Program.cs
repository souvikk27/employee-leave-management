using LeaveManagement.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebCoreServices(builder.Configuration);

var app = builder.Build();

app.UseAppPipeline();

app.Run();
