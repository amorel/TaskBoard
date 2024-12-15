using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;

namespace TaskBoard.Application.Tasks.Queries.GetAllTasks
{
    public class GetAllTasksQueryHandler : IQueryHandler<GetAllTasksQuery, IEnumerable<BoardTask>>
    {
        private readonly ITaskRepository _taskRepository;

        public GetAllTasksQueryHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<IEnumerable<BoardTask>> Handle(GetAllTasksQuery query, CancellationToken cancellationToken)
        {
            return await _taskRepository.GetAllAsync();
        }
    }
}