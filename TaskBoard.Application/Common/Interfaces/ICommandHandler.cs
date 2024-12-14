namespace TaskBoard.Application.Common.Interfaces
{
    /// <summary>
    /// Generic interface for command handlers
    /// </summary>
    public interface ICommandHandler<in TCommand, TResult>
    {
        Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    }
}