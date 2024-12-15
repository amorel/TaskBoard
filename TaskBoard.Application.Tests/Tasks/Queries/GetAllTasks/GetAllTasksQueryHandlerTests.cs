using Moq;
using FluentAssertions;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;
using TaskBoard.Domain.Enums;
using TaskBoard.Application.Tasks.Queries.GetAllTasks;

namespace TaskBoard.Application.Tests.Tasks.Queries.GetAllTasks
{
    public class GetAllTasksQueryHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly GetAllTasksQueryHandler _handler;

        public GetAllTasksQueryHandlerTests()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _handler = new GetAllTasksQueryHandler(_mockTaskRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllTasks()
        {
            // Arrange
            var expectedTasks = new List<BoardTask>
            {
                new BoardTask
                {
                    Title = "Task 1",
                    Description = "Description 1",
                    Status = TaskState.Todo
                },
                new BoardTask
                {
                    Title = "Task 2",
                    Description = "Description 2",
                    Status = TaskState.InProgress
                }
            };

            _mockTaskRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(expectedTasks);

            // Act
            var result = await _handler.Handle(new GetAllTasksQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedTasks);
            _mockTaskRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WithEmptyRepository_ShouldReturnEmptyList()
        {
            // Arrange
            _mockTaskRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<BoardTask>());

            // Act
            var result = await _handler.Handle(new GetAllTasksQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
            _mockTaskRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            _mockTaskRepository.Setup(repo => repo.GetAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(new GetAllTasksQuery(), CancellationToken.None));
        }
    }
}