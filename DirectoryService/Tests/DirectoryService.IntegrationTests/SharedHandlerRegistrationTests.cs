using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Handlers;

namespace DirectoryService.IntegrationTests;

public class SharedHandlerRegistrationTests
{
    [Fact]
    public void AddHandlersRegistersCommandAndQueryContracts()
    {
        var services = new ServiceCollection();
        services.AddHandlers(typeof(SharedHandlerRegistrationTests).Assembly);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<PingCommandHandler>(
            scope.ServiceProvider.GetRequiredService<ICommandHandler<PingCommand, string>>());
        Assert.IsType<PingQueryHandler>(
            scope.ServiceProvider.GetRequiredService<IQueryHandler<PingQuery, string>>());
    }

    public sealed record PingCommand : ICommand;

    public sealed record PingQuery : IQuery<string>;

    public sealed class PingCommandHandler : ICommandHandler<PingCommand, string>
    {
        public Task<string> Handle(PingCommand command, CancellationToken cancellationToken) =>
            Task.FromResult("pong");
    }

    public sealed class PingQueryHandler : IQueryHandler<PingQuery, string>
    {
        public Task<string> Handle(PingQuery query, CancellationToken cancellationToken) =>
            Task.FromResult("pong");
    }
}
