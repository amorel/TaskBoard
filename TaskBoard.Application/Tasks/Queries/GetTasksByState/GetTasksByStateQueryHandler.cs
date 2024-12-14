using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;

namespace TaskBoard.Application.Tasks.Queries.GetTasksByState
{
    public class GetTasksByStateQueryHandler : IQueryHandler<GetTasksByStateQuery, IEnumerable<BoardTask>>
    {
        private readonly ITaskRepository _taskRepository;

        public GetTasksByStateQueryHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<IEnumerable<BoardTask>> Handle(GetTasksByStateQuery query, CancellationToken cancellationToken)
        {
            return await _taskRepository.GetByStateAsync(query.State);
        }
    }
}
