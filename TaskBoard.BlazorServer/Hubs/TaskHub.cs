using Microsoft.AspNetCore.SignalR;
using TaskBoard.BlazorServer.ViewModels;
using TaskBoard.Domain.Enums;

namespace TaskBoard.BlazorServer.Hubs
{
    public class TaskHub : Hub
    {
        private readonly ILogger<TaskHub> _logger;

        public TaskHub(ILogger<TaskHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task TaskCreated(TaskViewModel task)
        {
            _logger.LogInformation("Broadcasting TaskCreated: {TaskId}", task.Id);
            await Clients.Others.SendAsync("ReceiveTaskCreated", task);
        }

        public async Task TaskUpdated(TaskViewModel task)
        {
            _logger.LogInformation("Broadcasting TaskUpdated: {TaskId}", task.Id);
            await Clients.Others.SendAsync("ReceiveTaskUpdated", task);
        }

        public async Task TaskDeleted(Guid taskId)
        {
            _logger.LogInformation("Broadcasting TaskDeleted: {TaskId}", taskId);
            await Clients.Others.SendAsync("ReceiveTaskDeleted", taskId);
        }

        public async Task TaskMoved(Guid taskId, TaskState newState)
        {
            _logger.LogInformation("Broadcasting TaskMoved: {TaskId} to {State}", taskId, newState);
            await Clients.Others.SendAsync("ReceiveTaskMoved", taskId, newState);
        }
    }
}