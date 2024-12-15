using FluentAssertions;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;
using TaskBoard.Infrastructure.Persistence.Repositories;
using TaskBoard.Infrastructure.Tests.Common;

namespace TaskBoard.Infrastructure.Tests.Repositories
{
    public class TaskRepositoryTests : TestDbContext
    {
        private readonly TaskRepository _repository;

        public TaskRepositoryTests()
        {
            _repository = new TaskRepository(Context);
        }

        [Fact]
        public async Task GetAllAsync_WithNoTasks_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithExistingTasks_ShouldReturnAllTasks()
        {
            // Arrange
            var tasks = new List<BoardTask>
            {
                new() { Title = "Task 1", Status = TaskState.Todo },
                new() { Title = "Task 2", Status = TaskState.InProgress }
            };

            await Context.Tasks.AddRangeAsync(tasks);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(tasks, options =>
                options.ComparingByMembers<BoardTask>());
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingTask_ShouldReturnTask()
        {
            // Arrange
            var task = new BoardTask
            {
                Title = "Test Task",
                Description = "Description",
                Status = TaskState.Todo
            };
            await Context.Tasks.AddAsync(task);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(task.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(task, options =>
                options.ComparingByMembers<BoardTask>());
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ValidTask_ShouldAddAndReturnTask()
        {
            // Arrange
            var task = new BoardTask
            {
                Title = "New Task",
                Description = "Description",
                Status = TaskState.Todo
            };

            // Act
            var result = await _repository.AddAsync(task);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBe(Guid.Empty);

            var savedTask = await Context.Tasks.FindAsync(result.Id);
            savedTask.Should().NotBeNull();
            savedTask.Should().BeEquivalentTo(task, options =>
                options.ComparingByMembers<BoardTask>());
        }

        [Fact]
        public async Task UpdateAsync_ExistingTask_ShouldUpdateAndReturnTask()
        {
            // Arrange
            var task = new BoardTask
            {
                Title = "Original Title",
                Description = "Original Description",
                Status = TaskState.Todo
            };
            await Context.Tasks.AddAsync(task);
            await Context.SaveChangesAsync();

            var originalId = task.Id;
            task.Title = "Updated Title";
            task.Description = "Updated Description";
            task.Status = TaskState.InProgress;

            // Act
            var result = await _repository.UpdateAsync(task);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(originalId);
            result.Title.Should().Be("Updated Title");
            result.Description.Should().Be("Updated Description");
            result.Status.Should().Be(TaskState.InProgress);

            var updatedTask = await Context.Tasks.FindAsync(originalId);
            updatedTask.Should().BeEquivalentTo(result);
        }

        [Fact]
        public async Task DeleteAsync_ExistingTask_ShouldRemoveTask()
        {
            // Arrange
            var task = new BoardTask
            {
                Title = "Task to Delete",
                Description = "Will be deleted",
                Status = TaskState.Todo
            };
            await Context.Tasks.AddAsync(task);
            await Context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(task.Id);

            // Assert
            var deletedTask = await Context.Tasks.FindAsync(task.Id);
            deletedTask.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_NonExistingTask_ShouldNotThrowException()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await _repository.DeleteAsync(nonExistingId);
        }

        [Theory]
        [InlineData(TaskState.Todo)]
        [InlineData(TaskState.InProgress)]
        [InlineData(TaskState.Done)]
        public async Task GetByStateAsync_ShouldReturnTasksWithMatchingState(TaskState state)
        {
            // Arrange
            var tasks = new List<BoardTask>
            {
                new() { Title = "Todo Task", Status = TaskState.Todo },
                new() { Title = "In Progress Task", Status = TaskState.InProgress },
                new() { Title = "Done Task", Status = TaskState.Done }
            };
            await Context.Tasks.AddRangeAsync(tasks);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByStateAsync(state);

            // Assert
            result.Should().NotBeNull();
            result.Should().OnlyContain(t => t.Status == state);
            result.Should().HaveCount(tasks.Count(t => t.Status == state));
        }

        [Fact]
        public async Task GetByStateAsync_WithNoMatchingTasks_ShouldReturnEmptyList()
        {
            // Arrange
            var tasks = new List<BoardTask>
            {
                new() { Title = "Todo Task", Status = TaskState.Todo },
                new() { Title = "In Progress Task", Status = TaskState.InProgress }
            };
            await Context.Tasks.AddRangeAsync(tasks);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByStateAsync(TaskState.Done);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}