using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;

namespace TaskBoard.Application.Tasks.Commands.UpdateTask
{
    public class UpdateTaskCommandHandler : ICommandHandler<UpdateTaskCommand, BoardTask>
    {
        private readonly ITaskRepository _taskRepository;

        public UpdateTaskCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<BoardTask> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
        {
            var existingTask = await _taskRepository.GetByIdAsync(command.Id);
            
            if (existingTask == null)
            {
                throw new InvalidOperationException($"Task with ID {command.Id} not found.");
            }

            existingTask.Title = command.Title;
            existingTask.Description = command.Description;
            existingTask.Status = command.Status;
            existingTask.LastModifiedAt = DateTime.UtcNow;

            return await _taskRepository.UpdateAsync(existingTask);
        }
    }
}
