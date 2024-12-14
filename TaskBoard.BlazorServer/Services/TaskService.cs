using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Application.Common.Models;
using TaskBoard.Application.Tasks.Commands.CreateTask;
using TaskBoard.Application.Tasks.Commands.DeleteTask;
using TaskBoard.Application.Tasks.Commands.UpdateTask;
using TaskBoard.Application.Tasks.Queries.GetAllTasks;
using TaskBoard.Application.Tasks.Queries.GetTaskById;
using TaskBoard.Application.Tasks.Queries.GetTasksByState;
using TaskBoard.BlazorServer.ViewModels;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;

namespace TaskBoard.BlazorServer.Services
{
    public class TaskService : ITaskService
    {
        private readonly IQueryHandler<GetAllTasksQuery, IEnumerable<BoardTask>> _getAllTasksHandler;
        private readonly IQueryHandler<GetTaskByIdQuery, BoardTask?> _getTaskByIdHandler;
        private readonly IQueryHandler<GetTasksByStateQuery, IEnumerable<BoardTask>> _getTasksByStateHandler;
        private readonly ICommandHandler<CreateTaskCommand, BoardTask> _createTaskHandler;
        private readonly ICommandHandler<UpdateTaskCommand, BoardTask> _updateTaskHandler;
        private readonly ICommandHandler<DeleteTaskCommand, Unit> _deleteTaskHandler;

        public event Action? OnChange;

        public TaskService(
            IQueryHandler<GetAllTasksQuery, IEnumerable<BoardTask>> getAllTasksHandler,
            IQueryHandler<GetTaskByIdQuery, BoardTask?> getTaskByIdHandler,
            IQueryHandler<GetTasksByStateQuery, IEnumerable<BoardTask>> getTasksByStateHandler,
            ICommandHandler<CreateTaskCommand, BoardTask> createTaskHandler,
            ICommandHandler<UpdateTaskCommand, BoardTask> updateTaskHandler,
            ICommandHandler<DeleteTaskCommand, Unit> deleteTaskHandler)
        {
            _getAllTasksHandler = getAllTasksHandler;
            _getTaskByIdHandler = getTaskByIdHandler;
            _getTasksByStateHandler = getTasksByStateHandler;
            _createTaskHandler = createTaskHandler;
            _updateTaskHandler = updateTaskHandler;
            _deleteTaskHandler = deleteTaskHandler;
        }

        public async Task<IEnumerable<TaskViewModel>> GetAllTasksAsync()
        {
            var tasks = await _getAllTasksHandler.Handle(new GetAllTasksQuery(), CancellationToken.None);
            return tasks.Select(MapToViewModel);
        }

        public async Task<TaskViewModel?> GetTaskByIdAsync(Guid id)
        {
            var task = await _getTaskByIdHandler.Handle(new GetTaskByIdQuery(id), CancellationToken.None);
            return task != null ? MapToViewModel(task) : null;
        }

        public async Task<IEnumerable<TaskViewModel>> GetTasksByStateAsync(TaskState state)
        {
            var tasks = await _getTasksByStateHandler.Handle(new GetTasksByStateQuery(state), CancellationToken.None);
            return tasks.Select(MapToViewModel);
        }

        public async Task<TaskViewModel> CreateTaskAsync(TaskViewModel viewModel)
        {
            var command = new CreateTaskCommand(viewModel.Title, viewModel.Description, viewModel.Status);
            var task = await _createTaskHandler.Handle(command, CancellationToken.None);
            NotifyStateChanged();
            return MapToViewModel(task);
        }

        public async Task<TaskViewModel> UpdateTaskAsync(TaskViewModel viewModel)
        {
            var command = new UpdateTaskCommand(viewModel.Id, viewModel.Title, viewModel.Description, viewModel.Status);
            var task = await _updateTaskHandler.Handle(command, CancellationToken.None);
            NotifyStateChanged();
            return MapToViewModel(task);
        }

        public async Task DeleteTaskAsync(Guid id)
        {
            await _deleteTaskHandler.Handle(new DeleteTaskCommand(id), CancellationToken.None);
            NotifyStateChanged();
        }

        private static TaskViewModel MapToViewModel(BoardTask task)
        {
            return new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                LastModifiedAt = task.LastModifiedAt
            };
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
