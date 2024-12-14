using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Application.Tasks.Commands.UpdateTask
{
    public record UpdateTaskCommand(
        Guid Id,
        string Title,
        string Description,
        TaskState Status
    ) : ICommand<BoardTask>;
}
