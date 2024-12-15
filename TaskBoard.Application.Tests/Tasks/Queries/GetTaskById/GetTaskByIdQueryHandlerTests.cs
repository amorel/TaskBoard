using Moq;
using FluentAssertions;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;
using TaskBoard.Domain.Enums;
using TaskBoard.Application.Tasks.Queries.GetTaskById;

namespace TaskBoard.Application.Tests.Tasks.Queries.GetTaskById
{
    public class GetTaskByIdQueryHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly GetTaskByIdQueryHandler _handler;

        public GetTaskByIdQueryHandlerTests()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _handler = new GetTaskByIdQueryHandler(_mockTaskRepository.Object);
        }

        [Fact]
        public async Task Handle_WithExistingId_ShouldReturnTask()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var expectedTask = new BoardTask
            {
                Id = taskId,
                Title = "Test Task",
                Description = "Test Description",
                Status = TaskState.Todo
            };

            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ReturnsAsync(expectedTask);

            // Act
            var result = await _handler.Handle(new GetTaskByIdQuery(taskId), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedTask);
            _mockTaskRepository.Verify(repo => repo.GetByIdAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(nonExistingId))
                .ReturnsAsync((BoardTask?)null);

            // Act
            var result = await _handler.Handle(new GetTaskByIdQuery(nonExistingId), CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _mockTaskRepository.Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _mockTaskRepository.Setup(repo => repo.GetByIdAsync(taskId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(new GetTaskByIdQuery(taskId), CancellationToken.None));
        }
    }
}