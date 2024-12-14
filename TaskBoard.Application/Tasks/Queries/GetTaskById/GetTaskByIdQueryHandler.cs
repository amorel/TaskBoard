using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;

namespace TaskBoard.Application.Tasks.Queries.GetTaskById
{
    public class GetTaskByIdQueryHandler : IQueryHandler<GetTaskByIdQuery, BoardTask?>
    {
        private readonly ITaskRepository _taskRepository;

        public GetTaskByIdQueryHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<BoardTask?> Handle(GetTaskByIdQuery query, CancellationToken cancellationToken)
        {
            return await _taskRepository.GetByIdAsync(query.Id);
        }
    }
}
