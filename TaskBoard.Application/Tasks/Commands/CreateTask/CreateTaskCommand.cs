using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Application.Tasks.Commands.CreateTask
{
    public record CreateTaskCommand(
        string Title,
        string Description,
        TaskState Status
    ) : ICommand<BoardTask>;
}
