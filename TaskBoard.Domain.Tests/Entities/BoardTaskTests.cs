using FluentAssertions;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;
using Xunit;

namespace TaskBoard.Domain.Tests.Entities
{
    public class BoardTaskTests
    {
        [Fact]
        public void CreateBoardTask_WithValidData_ShouldCreateTaskWithCorrectProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var title = "Test Task";
            var description = "Test Description";
            var status = TaskState.Todo;
            var createdAt = DateTime.UtcNow;
            var lastModifiedAt = DateTime.UtcNow;

            // Act
            var task = new BoardTask
            {
                Id = id,
                Title = title,
                Description = description,
                Status = status,
                CreatedAt = createdAt,
                LastModifiedAt = lastModifiedAt
            };

            // Assert
            task.Id.Should().Be(id);
            task.Title.Should().Be(title);
            task.Description.Should().Be(description);
            task.Status.Should().Be(status);
            task.CreatedAt.Should().Be(createdAt);
            task.LastModifiedAt.Should().Be(lastModifiedAt);
        }

        [Theory]
        [InlineData(TaskState.Todo)]
        [InlineData(TaskState.InProgress)]
        [InlineData(TaskState.Done)]
        public void CreateBoardTask_WithDifferentTaskStates_ShouldAcceptAllValidStates(TaskState state)
        {
            // Arrange & Act
            var task = new BoardTask { Status = state };

            // Assert
            task.Status.Should().Be(state);
        }

        [Fact]
        public void BoardTask_ModificationTime_ShouldBeUpdatable()
        {
            // Arrange
            var task = new BoardTask
            {
                LastModifiedAt = DateTime.UtcNow
            };
            var newModificationTime = DateTime.UtcNow.AddHours(1);

            // Act
            task.LastModifiedAt = newModificationTime;

            // Assert
            task.LastModifiedAt.Should().Be(newModificationTime);
        }

        [Fact]
        public void CreateBoardTask_WithEmptyTitle_ShouldStillBeValid()
        {
            // Arrange & Act
            var task = new BoardTask { Title = string.Empty };

            // Assert
            task.Title.Should().BeEmpty();
        }

        [Fact]
        public void CreateBoardTask_WithEmptyDescription_ShouldStillBeValid()
        {
            // Arrange & Act
            var task = new BoardTask { Description = string.Empty };

            // Assert
            task.Description.Should().BeEmpty();
        }

        [Fact]
        public void CreateBoardTask_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var task = new BoardTask();

            // Assert
            task.Id.Should().NotBe(Guid.Empty);
            task.Title.Should().BeEmpty();
            task.Description.Should().BeEmpty();
            task.Status.Should().Be(TaskState.Todo); // Vérifie que l'état par défaut est Todo
            task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            task.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void TwoTasks_WithSameProperties_ShouldNotBeEqual()
        {
            // Arrange
            var task1 = new BoardTask
            {
                Title = "Test",
                Description = "Description",
                Status = TaskState.Todo
            };

            var task2 = new BoardTask
            {
                Title = "Test",
                Description = "Description",
                Status = TaskState.Todo
            };

            // Assert
            task1.Should().NotBe(task2); // Car les IDs sont différents
            task1.Id.Should().NotBe(task2.Id);
        }

        [Fact]
        public void UpdateTask_ShouldUpdateLastModifiedAt()
        {
            // Arrange
            var task = new BoardTask
            {
                Title = "Initial Title",
                LastModifiedAt = DateTime.UtcNow.AddHours(-1) // Une heure dans le passé
            };
            var initialModifiedAt = task.LastModifiedAt;

            // Act
            task.Title = "Updated Title";
            task.LastModifiedAt = DateTime.UtcNow;

            // Assert
            task.LastModifiedAt.Should().BeAfter(initialModifiedAt);
        }
    }
}