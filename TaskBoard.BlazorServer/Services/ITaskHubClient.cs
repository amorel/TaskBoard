using TaskBoard.BlazorServer.ViewModels;
using TaskBoard.Domain.Enums;

namespace TaskBoard.BlazorServer.Services
{
    public interface ITaskHubClient
    {
        event Action<TaskViewModel> OnTaskCreated;
        event Action<TaskViewModel> OnTaskUpdated;
        event Action<Guid> OnTaskDeleted;
        event Action<Guid, TaskState> OnTaskMoved;
        Task NotifyTaskMoved(Guid taskId, TaskState newState);

        Task NotifyTaskCreated(TaskViewModel task);
        Task NotifyTaskUpdated(TaskViewModel task);
        Task NotifyTaskDeleted(Guid taskId);
        Task Connect();
        Task Disconnect();
    }
}