using Moq;
using FluentAssertions;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;
using TaskBoard.Domain.Enums;
using TaskBoard.Application.Tasks.Commands.UpdateTask;

namespace TaskBoard.Application.Tests.Tasks.Commands.UpdateTask
{
    public class UpdateTaskCommandHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly UpdateTaskCommandHandler _handler;

        public UpdateTaskCommandHandlerTests()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _handler = new UpdateTaskCommandHandler(_mockTaskRepository.Object);
        }

        [Fact]
        public async Task Handle_ExistingTask_ShouldUpdateAndReturnTask()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var oldDate = DateTime.UtcNow.AddHours(-1);
            var existingTask = new BoardTask
            {
                Id = taskId,
                Title = "Old Title",
                Description = "Old Description",
                Status = TaskState.Todo,
                CreatedAt = oldDate,
                LastModifiedAt = oldDate
            };

            var command = new UpdateTaskCommand(
                taskId,
                "New Title",
                "New Description",
                TaskState.InProgress
            );

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<BoardTask>()))
                .ReturnsAsync((BoardTask task) =>
                {
                    task.LastModifiedAt = DateTime.UtcNow; // Mettre à jour la date explicitement
                    return task;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(taskId);
            result.Title.Should().Be(command.Title);
            result.Description.Should().Be(command.Description);
            result.Status.Should().Be(command.Status);
            result.CreatedAt.Should().Be(existingTask.CreatedAt);
            result.LastModifiedAt.Should().NotBe(oldDate);
            result.LastModifiedAt.Should().BeAfter(oldDate);

            _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.Is<BoardTask>(t =>
                t.Id == taskId &&
                t.Title == command.Title &&
                t.Description == command.Description &&
                t.Status == command.Status)), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingTask_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var command = new UpdateTaskCommand(
                taskId,
                "New Title",
                "New Description",
                TaskState.InProgress
            );

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync((BoardTask?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _mockTaskRepository.Verify(repo => repo.UpdateAsync(It.IsAny<BoardTask>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existingTask = new BoardTask { Id = taskId };
            var command = new UpdateTaskCommand(
                taskId,
                "New Title",
                "New Description",
                TaskState.InProgress
            );

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<BoardTask>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Theory]
        [InlineData("", "Description", TaskState.Todo)]
        [InlineData("Title", "", TaskState.InProgress)]
        public async Task Handle_WithEmptyStrings_ShouldUpdateTask(string title, string description, TaskState status)
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existingTask = new BoardTask { Id = taskId };
            var command = new UpdateTaskCommand(taskId, title, description, status);

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _mockTaskRepository.Setup(repo => repo.UpdateAsync(It.IsAny<BoardTask>()))
                .ReturnsAsync((BoardTask task) => task);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(title);
            result.Description.Should().Be(description);
            result.Status.Should().Be(status);
        }
    }
}