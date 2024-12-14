namespace TaskBoard.Application.Common.Interfaces
{
    /// <summary>
    /// Generic interface for query handlers
    /// </summary>
    public interface IQueryHandler<in TQuery, TResult>
    {
        Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
    }
}