using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using TaskBoard.BlazorServer.ViewModels;
using TaskBoard.Domain.Enums;

namespace TaskBoard.BlazorServer.Services
{
    public class TaskHubClient : ITaskHubClient, IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
        private readonly ILogger<TaskHubClient> _logger;
        private bool _isConnected;
        private bool _disposed;
        private readonly string _hubUrl;
        private readonly IConfiguration _configuration;

        public event Action<TaskViewModel>? OnTaskCreated;
        public event Action<TaskViewModel>? OnTaskUpdated;
        public event Action<Guid>? OnTaskDeleted;
        public event Action<Guid, TaskState>? OnTaskMoved;

        public TaskHubClient(IConfiguration configuration, ILogger<TaskHubClient> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:5201";
            _hubUrl = $"{baseUrl}/taskhub";

            _logger.LogInformation("Initializing SignalR hub with URL: {_hubUrl}", _hubUrl);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.Transports = HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling;
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .Build();

            ConfigureHubEvents();
        }

        private void ConfigureHubEvents()
        {
            _hubConnection.Closed += async (error) =>
            {
                _isConnected = false;
                _logger.LogWarning(error, "SignalR connection closed");
                await ReconnectWithRetryAsync();
            };

            _hubConnection.Reconnecting += (error) =>
            {
                _isConnected = false;
                _logger.LogInformation(error, "SignalR attempting to reconnect");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                _isConnected = true;
                _logger.LogInformation("SignalR reconnected with ID: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _hubConnection.On<TaskViewModel>("ReceiveTaskCreated", (task) =>
            {
                _logger.LogInformation("Task created received: {TaskId}", task.Id);
                OnTaskCreated?.Invoke(task);
            });

            _hubConnection.On<TaskViewModel>("ReceiveTaskUpdated", (task) =>
            {
                _logger.LogInformation("Task updated received: {TaskId}", task.Id);
                OnTaskUpdated?.Invoke(task);
            });

            _hubConnection.On<Guid>("ReceiveTaskDeleted", (taskId) =>
            {
                _logger.LogInformation("Task deleted received: {TaskId}", taskId);
                OnTaskDeleted?.Invoke(taskId);
            });

            _hubConnection.On<Guid, TaskState>("ReceiveTaskMoved", (taskId, newState) =>
            {
                _logger.LogInformation("Task moved received: {TaskId} to {State}", taskId, newState);
                OnTaskMoved?.Invoke(taskId, newState);
            });
        }

        public async Task NotifyTaskMoved(Guid taskId, TaskState newState)
        {
            try
            {
                await _hubConnection.InvokeAsync("TaskMoved", taskId, newState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying task moved");
                throw;
            }
        }

        private async Task ReconnectWithRetryAsync()
        {
            try
            {
                await Connect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconnect to SignalR hub");
            }
        }

        public async Task NotifyTaskCreated(TaskViewModel task)
        {
            try
            {
                await _hubConnection.InvokeAsync("TaskCreated", task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying task created");
            }
        }

        public async Task NotifyTaskUpdated(TaskViewModel task)
        {
            try
            {
                await _hubConnection.InvokeAsync("TaskUpdated", task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying task updated");
            }
        }

        public async Task NotifyTaskDeleted(Guid taskId)
        {
            try
            {
                await _hubConnection.InvokeAsync("TaskDeleted", taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying task deleted");
            }
        }

        public async Task Connect()
        {
            try
            {
                await _connectionLock.WaitAsync();

                if (!_disposed && !_isConnected)
                {
                    _logger.LogInformation("Attempting to connect to SignalR hub...");
                    await _hubConnection.StartAsync();
                    _isConnected = true;
                    _logger.LogInformation("Successfully connected to SignalR hub");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to SignalR hub");
                _isConnected = false;
                throw;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async Task Disconnect()
        {
            if (_disposed) return;

            try
            {
                if (_connectionLock.CurrentCount == 0)
                {
                    _logger.LogWarning("Connection lock is already held during disconnect");
                    return;
                }

                await _connectionLock.WaitAsync();
                if (_isConnected)
                {
                    await _hubConnection.StopAsync();
                    _isConnected = false;
                    _logger.LogInformation("Disconnected from SignalR Hub");
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignorer si déjà disposé
                _logger.LogInformation("Hub connection already disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from SignalR Hub");
            }
            finally
            {
                if (!_disposed)
                {
                    _connectionLock.Release();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                await Disconnect();
            }
            finally
            {
                await _hubConnection.DisposeAsync();
                _connectionLock.Dispose();
            }
        }
    }
}