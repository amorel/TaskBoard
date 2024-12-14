using TaskBoard.BlazorServer.ViewModels;
using TaskBoard.Domain.Enums;

namespace TaskBoard.BlazorServer.Services
{
    public interface ITaskService
    {
        event Action? OnChange;
        Task<IEnumerable<TaskViewModel>> GetAllTasksAsync();
        Task<TaskViewModel?> GetTaskByIdAsync(Guid id);
        Task<IEnumerable<TaskViewModel>> GetTasksByStateAsync(TaskState state);
        Task<TaskViewModel> CreateTaskAsync(TaskViewModel task);
        Task<TaskViewModel> UpdateTaskAsync(TaskViewModel task);
        Task DeleteTaskAsync(Guid id);
    }
}
