using System.Diagnostics;
using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Persistence.Repositories;
using TaskBoard.Infrastructure.Tests.Common;
using FluentAssertions;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Infrastructure.Tests.Performance
{
    public class RepositoryPerformanceTests : TestDbContext
    {
        private readonly TaskRepository _repository;

        public RepositoryPerformanceTests()
        {
            _repository = new TaskRepository(Context);
        }

        [Fact]
        public async Task GetAllAsync_ShouldPerformWellWithLargeDataset()
        {
            // Arrange
            var tasks = Enumerable.Range(0, 1000)
                .Select(i => new BoardTask 
                { 
                    Title = $"Task {i}",
                    Description = $"Description {i}",
                    Status = TaskState.Todo
                })
                .ToList();

            await Context.Tasks.AddRangeAsync(tasks);
            await Context.SaveChangesAsync();

            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var result = await _repository.GetAllAsync();
            stopwatch.Stop();

            // Assert
            result.Should().HaveCount(1000);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // moins d'une seconde
        }
    }
}