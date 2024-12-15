using Moq;
using FluentAssertions;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;
using TaskBoard.Domain.Enums;
using TaskBoard.Application.Tasks.Queries.GetTasksByState;

namespace TaskBoard.Application.Tests.Tasks.Queries.GetTasksByState
{
    public class GetTasksByStateQueryHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly GetTasksByStateQueryHandler _handler;

        public GetTasksByStateQueryHandlerTests()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _handler = new GetTasksByStateQueryHandler(_mockTaskRepository.Object);
        }

        [Theory]
        [InlineData(TaskState.Todo)]
        [InlineData(TaskState.InProgress)]
        [InlineData(TaskState.Done)]
        public async Task Handle_ShouldReturnTasksWithSpecificState(TaskState state)
        {
            // Arrange
            var tasks = new List<BoardTask>
            {
                new BoardTask { Title = "Task 1", Status = state },
                new BoardTask { Title = "Task 2", Status = state }
            };

            _mockTaskRepository.Setup(repo => repo.GetByStateAsync(state))
                .ReturnsAsync(tasks);

            // Act
            var result = await _handler.Handle(new GetTasksByStateQuery(state), CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(t => t.Status == state);
            _mockTaskRepository.Verify(repo => repo.GetByStateAsync(state), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNoTasksInState_ShouldReturnEmptyList()
        {
            // Arrange
            var state = TaskState.Todo;
            _mockTaskRepository.Setup(repo => repo.GetByStateAsync(state))
                .ReturnsAsync(new List<BoardTask>());

            // Act
            var result = await _handler.Handle(new GetTasksByStateQuery(state), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
            _mockTaskRepository.Verify(repo => repo.GetByStateAsync(state), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var state = TaskState.Todo;
            _mockTaskRepository.Setup(repo => repo.GetByStateAsync(state))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(new GetTasksByStateQuery(state), CancellationToken.None));
        }
    }
}