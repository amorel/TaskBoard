using Moq;
using FluentAssertions;
using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Application.Common.Models;
using TaskBoard.Application.Tasks.Queries.GetAllTasks;
using TaskBoard.Application.Tasks.Queries.GetTaskById;
using TaskBoard.Application.Tasks.Queries.GetTasksByState;
using TaskBoard.Application.Tasks.Commands.CreateTask;
using TaskBoard.Application.Tasks.Commands.UpdateTask;
using TaskBoard.Application.Tasks.Commands.DeleteTask;
using TaskBoard.BlazorServer.Services;
using TaskBoard.BlazorServer.ViewModels;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;

namespace TaskBoard.BlazorServer.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<IQueryHandler<GetAllTasksQuery, IEnumerable<BoardTask>>> _mockGetAllTasksHandler;
        private readonly Mock<IQueryHandler<GetTaskByIdQuery, BoardTask?>> _mockGetTaskByIdHandler;
        private readonly Mock<IQueryHandler<GetTasksByStateQuery, IEnumerable<BoardTask>>> _mockGetTasksByStateHandler;
        private readonly Mock<ICommandHandler<CreateTaskCommand, BoardTask>> _mockCreateTaskHandler;
        private readonly Mock<ICommandHandler<UpdateTaskCommand, BoardTask>> _mockUpdateTaskHandler;
        private readonly Mock<ICommandHandler<DeleteTaskCommand, Unit>> _mockDeleteTaskHandler;
        private readonly TaskService _service;
        private bool _onChangeWasCalled;

        public TaskServiceTests()
        {
            _mockGetAllTasksHandler = new Mock<IQueryHandler<GetAllTasksQuery, IEnumerable<BoardTask>>>();
            _mockGetTaskByIdHandler = new Mock<IQueryHandler<GetTaskByIdQuery, BoardTask?>>();
            _mockGetTasksByStateHandler = new Mock<IQueryHandler<GetTasksByStateQuery, IEnumerable<BoardTask>>>();
            _mockCreateTaskHandler = new Mock<ICommandHandler<CreateTaskCommand, BoardTask>>();
            _mockUpdateTaskHandler = new Mock<ICommandHandler<UpdateTaskCommand, BoardTask>>();
            _mockDeleteTaskHandler = new Mock<ICommandHandler<DeleteTaskCommand, Unit>>();

            _service = new TaskService(
                _mockGetAllTasksHandler.Object,
                _mockGetTaskByIdHandler.Object,
                _mockGetTasksByStateHandler.Object,
                _mockCreateTaskHandler.Object,
                _mockUpdateTaskHandler.Object,
                _mockDeleteTaskHandler.Object
            );

            _service.OnChange += () => _onChangeWasCalled = true;
        }

        [Fact]
        public async Task GetAllTasksAsync_ShouldReturnMappedViewModels()
        {
            // Arrange
            var tasks = new List<BoardTask>
            {
                new() { Id = Guid.NewGuid(), Title = "Task 1", Status = TaskState.Todo },
                new() { Id = Guid.NewGuid(), Title = "Task 2", Status = TaskState.InProgress }
            };

            _mockGetAllTasksHandler
                .Setup(h => h.Handle(It.IsAny<GetAllTasksQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);

            // Act
            var result = await _service.GetAllTasksAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllBeOfType<TaskViewModel>();
            result.Select(t => t.Title).Should().BeEquivalentTo(tasks.Select(t => t.Title));
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithExistingId_ShouldReturnMappedViewModel()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = new BoardTask
            {
                Id = taskId,
                Title = "Test Task",
                Status = TaskState.Todo
            };

            _mockGetTaskByIdHandler
                .Setup(h => h.Handle(It.Is<GetTaskByIdQuery>(q => q.Id == taskId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);

            // Act
            var result = await _service.GetTaskByIdAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(taskId);
            result.Title.Should().Be(task.Title);
            result.Status.Should().Be(task.Status);
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldCallHandlerAndNotifyChange()
        {
            // Arrange
            var viewModel = new TaskViewModel
            {
                Title = "New Task",
                Description = "Description",
                Status = TaskState.Todo
            };

            var createdTask = new BoardTask
            {
                Id = Guid.NewGuid(),
                Title = viewModel.Title,
                Description = viewModel.Description,
                Status = viewModel.Status
            };

            _mockCreateTaskHandler
                .Setup(h => h.Handle(It.IsAny<CreateTaskCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTask);

            // Act
            var result = await _service.CreateTaskAsync(viewModel);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(viewModel.Title);
            result.Description.Should().Be(viewModel.Description);
            result.Status.Should().Be(viewModel.Status);
            _onChangeWasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateTaskAsync_ShouldCallHandlerAndNotifyChange()
        {
            // Arrange
            var viewModel = new TaskViewModel
            {
                Id = Guid.NewGuid(),
                Title = "Updated Task",
                Description = "Updated Description",
                Status = TaskState.InProgress
            };

            var updatedTask = new BoardTask
            {
                Id = viewModel.Id,
                Title = viewModel.Title,
                Description = viewModel.Description,
                Status = viewModel.Status
            };

            _mockUpdateTaskHandler
                .Setup(h => h.Handle(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedTask);

            // Act
            var result = await _service.UpdateTaskAsync(viewModel);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(viewModel.Id);
            result.Title.Should().Be(viewModel.Title);
            result.Description.Should().Be(viewModel.Description);
            result.Status.Should().Be(viewModel.Status);
            _onChangeWasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldCallHandlerAndNotifyChange()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _mockDeleteTaskHandler
                .Setup(h => h.Handle(It.Is<DeleteTaskCommand>(c => c.Id == taskId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            await _service.DeleteTaskAsync(taskId);

            // Assert
            _mockDeleteTaskHandler.Verify(
                h => h.Handle(It.Is<DeleteTaskCommand>(c => c.Id == taskId), It.IsAny<CancellationToken>()),
                Times.Once);
            _onChangeWasCalled.Should().BeTrue();
        }

        [Theory]
        [InlineData(TaskState.Todo)]
        [InlineData(TaskState.InProgress)]
        [InlineData(TaskState.Done)]
        public async Task GetTasksByStateAsync_ShouldReturnTasksWithSpecificState(TaskState state)
        {
            // Arrange
            var tasks = new List<BoardTask>
            {
                new() { Id = Guid.NewGuid(), Title = "Task 1", Status = state },
                new() { Id = Guid.NewGuid(), Title = "Task 2", Status = state }
            };

            _mockGetTasksByStateHandler
                .Setup(h => h.Handle(It.Is<GetTasksByStateQuery>(q => q.State == state), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);

            // Act
            var result = await _service.GetTasksByStateAsync(state);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllBeOfType<TaskViewModel>();
            result.Should().AllSatisfy(t => t.Status.Should().Be(state));
        }
    }
}