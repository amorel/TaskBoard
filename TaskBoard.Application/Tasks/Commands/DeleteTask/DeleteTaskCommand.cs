using TaskBoard.Application.Common.Interfaces;

namespace TaskBoard.Application.Tasks.Commands.DeleteTask
{
    public record DeleteTaskCommand(Guid Id) : ICommand<Unit>;
}
