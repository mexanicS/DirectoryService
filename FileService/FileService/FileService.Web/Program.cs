using FileService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<FileServiceDbContext>();

    app.Logger.LogInformation("Applying database migrations");

    await dbContext.Database.MigrateAsync();

    app.Logger.LogInformation("Database migrations applied");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();