using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Application.Tasks.Queries.GetTasksByState
{
    public record GetTasksByStateQuery(TaskState State) : IQuery<IEnumerable<BoardTask>>;
}
