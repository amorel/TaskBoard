using Moq;
using FluentAssertions;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;
using TaskBoard.Application.Tasks.Commands.DeleteTask;
using TaskBoard.Application.Common.Models;

namespace TaskBoard.Application.Tests.Tasks.Commands.DeleteTask
{
    public class DeleteTaskCommandHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly DeleteTaskCommandHandler _handler;

        public DeleteTaskCommandHandlerTests()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _handler = new DeleteTaskCommandHandler(_mockTaskRepository.Object);
        }

        [Fact]
        public async Task Handle_ExistingTask_ShouldDeleteAndReturnUnit()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _mockTaskRepository.Setup(repo => repo.DeleteAsync(taskId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(new DeleteTaskCommand(taskId), CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _mockTaskRepository.Verify(repo => repo.DeleteAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _mockTaskRepository.Setup(repo => repo.DeleteAsync(taskId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(new DeleteTaskCommand(taskId), CancellationToken.None));

            _mockTaskRepository.Verify(repo => repo.DeleteAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task Handle_WithEmptyGuid_ShouldStillCallRepository()
        {
            // Arrange
            var taskId = Guid.Empty;
            _mockTaskRepository.Setup(repo => repo.DeleteAsync(taskId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(new DeleteTaskCommand(taskId), CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _mockTaskRepository.Verify(repo => repo.DeleteAsync(taskId), Times.Once);
        }
    }
}