using Moq;
using FluentAssertions;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Interfaces;
using TaskBoard.Domain.Enums;
using TaskBoard.Application.Tasks.Commands.CreateTask;

namespace TaskBoard.Application.Tests.Tasks.Commands.CreateTask
{
    public class CreateTaskCommandHandlerTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly CreateTaskCommandHandler _handler;

        public CreateTaskCommandHandlerTests()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _handler = new CreateTaskCommandHandler(_mockTaskRepository.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldCreateAndReturnTask()
        {
            // Arrange
            var command = new CreateTaskCommand(
                "Test Task",
                "Test Description",
                TaskState.Todo
            );

            var expectedTask = new BoardTask
            {
                Title = command.Title,
                Description = command.Description,
                Status = command.Status
            };

            _mockTaskRepository.Setup(repo => repo.AddAsync(It.IsAny<BoardTask>()))
                .ReturnsAsync((BoardTask task) => task);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(command.Title);
            result.Description.Should().Be(command.Description);
            result.Status.Should().Be(command.Status);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            
            _mockTaskRepository.Verify(repo => 
                repo.AddAsync(It.Is<BoardTask>(t => 
                    t.Title == command.Title && 
                    t.Description == command.Description && 
                    t.Status == command.Status)), 
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var command = new CreateTaskCommand(
                "Test Task",
                "Test Description",
                TaskState.Todo
            );

            _mockTaskRepository.Setup(repo => repo.AddAsync(It.IsAny<BoardTask>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(command, CancellationToken.None));
        }

        [Theory]
        [InlineData("", "Description", TaskState.Todo)]
        [InlineData("Title", "", TaskState.InProgress)]
        public async Task Handle_WithEmptyStrings_ShouldStillCreateTask(string title, string description, TaskState status)
        {
            // Arrange
            var command = new CreateTaskCommand(title, description, status);

            _mockTaskRepository.Setup(repo => repo.AddAsync(It.IsAny<BoardTask>()))
                .ReturnsAsync((BoardTask task) => task);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(title);
            result.Description.Should().Be(description);
            
            _mockTaskRepository.Verify(repo => 
                repo.AddAsync(It.Is<BoardTask>(t => 
                    t.Title == title && 
                    t.Description == description)), 
                Times.Once);
        }
    }
}