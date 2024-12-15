using Microsoft.EntityFrameworkCore;
using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Tests.Common;
using FluentAssertions;
using TaskBoard.Infrastructure.Persistence;

namespace TaskBoard.Infrastructure.Tests.Persistence
{
    public class DbContextConfigurationTests : TestDbContext
    {
        [Fact]
        public void TaskConfiguration_ShouldHaveCorrectTableName()
        {
            // Arrange & Act
            var entityType = Context.Model.FindEntityType(typeof(BoardTask));

            // Assert
            entityType.Should().NotBeNull();
            entityType!.GetTableName().Should().Be("Tasks");
        }

        [Fact]
        public void TaskConfiguration_ShouldHaveRequiredProperties()
        {
            // Arrange & Act
            var entityType = Context.Model.FindEntityType(typeof(BoardTask));

            // Assert
            entityType.Should().NotBeNull();

            var idProperty = entityType!.FindProperty(nameof(BoardTask.Id));
            idProperty.Should().NotBeNull();
            idProperty!.IsKey().Should().BeTrue();

            var titleProperty = entityType.FindProperty(nameof(BoardTask.Title));
            titleProperty.Should().NotBeNull();
            titleProperty!.IsNullable.Should().BeFalse();

            var descriptionProperty = entityType.FindProperty(nameof(BoardTask.Description));
            descriptionProperty.Should().NotBeNull();
            descriptionProperty!.IsNullable.Should().BeFalse();
        }

        [Fact]
        public async Task TaskConfiguration_ShouldEnforceUniqueId()
        {
            // Arrange
            var id = Guid.NewGuid();
            var task1 = new BoardTask
            {
                Id = id,
                Title = "Task 1"
            };
            await Context.Tasks.AddAsync(task1);
            await Context.SaveChangesAsync();

            // Créer un nouveau contexte pour éviter le tracking
            await using var newContext = new TaskBoardDbContext(GetContextOptions());
            var task2 = new BoardTask
            {
                Id = id,
                Title = "Task 2"
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await newContext.Tasks.AddAsync(task2);
                await newContext.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task TaskConfiguration_ShouldAutomaticallySetDates()
        {
            // Arrange
            var task = new BoardTask { Title = "Test Task" };

            // Act
            await Context.Tasks.AddAsync(task);
            await Context.SaveChangesAsync();

            // Assert
            var savedTask = await Context.Tasks.FindAsync(task.Id);
            savedTask.Should().NotBeNull();
            savedTask!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            savedTask.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}