namespace Shared.Core.Handlers;

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}
