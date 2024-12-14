using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Tasks.Queries.GetAllTasks
{
    /// <summary>
    /// Query to get all tasks
    /// </summary>
    public record GetAllTasksQuery : IQuery<IEnumerable<BoardTask>>;
}