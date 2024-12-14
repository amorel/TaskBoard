using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Application.Common.Models;
using TaskBoard.Domain.Interfaces;

namespace TaskBoard.Application.Tasks.Commands.DeleteTask
{
    public class DeleteTaskCommandHandler : ICommandHandler<DeleteTaskCommand, Unit>
    {
        private readonly ITaskRepository _taskRepository;

        public DeleteTaskCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<Unit> Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
        {
            await _taskRepository.DeleteAsync(command.Id);
            return Unit.Value;
        }
    }
}
