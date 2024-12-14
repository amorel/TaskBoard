using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Application.Common.Models;

namespace TaskBoard.Application.Tasks.Commands.DeleteTask
{
    public record DeleteTaskCommand(Guid Id) : ICommand<Unit>;
}
