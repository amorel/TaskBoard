using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;

namespace TaskBoard.Application.Tasks.Commands.CreateTask
{
    public class CreateTaskCommandHandler : ICommandHandler<CreateTaskCommand, BoardTask>
    {
        private readonly ITaskRepository _taskRepository;

        public CreateTaskCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<BoardTask> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
        {
            var task = new BoardTask
            {
                Id = Guid.NewGuid(),
                Title = command.Title,
                Description = command.Description,
                Status = command.Status,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            return await _taskRepository.AddAsync(task);
        }
    }
}
