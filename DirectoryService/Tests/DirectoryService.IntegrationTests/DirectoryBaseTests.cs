using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public abstract class DirectoryBaseTests<THandler> : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime 
    where THandler : notnull
{
    private readonly Func<Task> _resetDatabase;
    protected IServiceProvider Services { get; set; }
    
    protected DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }
    
    protected async Task<T> ExecuteContext<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        
        return await action(context);
    }
    
    protected async Task ExecuteContext(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        
        await action(context);
    }
    
    protected async Task<TResult> ExecuteHandler<TResult>(Func<THandler, Task<TResult>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<THandler>();
        
        return await action(sut);
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }
}