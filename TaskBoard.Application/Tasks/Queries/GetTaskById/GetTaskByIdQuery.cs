using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Tasks.Queries.GetTaskById
{
    public record GetTaskByIdQuery(Guid Id) : IQuery<BoardTask?>;
}
