using DirectoryService.Application;
using DirectoryService.Infrastructure;
using DirectoryService.Presentation.Controllers.DepartmentChildrenController;
using DirectoryService.Presentation.Controllers.DepartmentsController;
using DirectoryService.Presentation.Controllers.LocationsController;
using DirectoryService.Presentation.Controllers.PositionsController;
using Shared.Framework.Middlewares;
using DirectoryService.Presentation.Observability;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Debug()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq") 
                 ?? throw new ArgumentNullException($"Seq"))
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .CreateLogger();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(DepartmentController).Assembly)
    .AddApplicationPart(typeof(LocationController).Assembly)
    .AddApplicationPart(typeof(PositionController).Assembly)
    .AddApplicationPart(typeof(DepartmentTreeController).Assembly);

builder.Services.AddOpenApi();

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddSpeciesApplication()
    .AddSerilog();

builder.Services.AddDirectoryServiceObservability(builder.Environment);

builder.Services.AddHttpLogging(o =>
{
    o.CombineLogs = true;
});

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<DirectoryServiceDbContext>();

        Log.Information("Applying database migrations");
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied");
    }
    catch (Exception exception)
    {
        Log.Fatal(exception, "Database migration failed. Backend startup aborted");
        await Log.CloseAndFlushAsync();
        throw;
    }
}

app.UseExceptionMiddleware();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapDirectoryServiceMetrics();

app.MapGet("/health/ready", async (
    DirectoryServiceDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database
        .CanConnectAsync(cancellationToken);

    return canConnect
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program;
}
