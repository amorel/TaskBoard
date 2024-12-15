using Microsoft.EntityFrameworkCore;
using TaskBoard.Infrastructure.Tests.Common;
using FluentAssertions;

namespace TaskBoard.Infrastructure.Tests.Persistence
{
    public class MigrationTests : TestDbContext
    {
        [Fact]
        public async Task ApplyMigrations_ShouldCreateDatabase()
        {
            // Arrange
            await Context.Database.EnsureDeletedAsync();

            // Act
            await Context.Database.MigrateAsync();

            // Assert
            (await Context.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
            (await Context.Database.GetAppliedMigrationsAsync()).Should().NotBeEmpty();
        }

        [Fact]
        public async Task Database_ShouldHaveCorrectSchema()
        {
            // Arrange
            await Context.Database.EnsureDeletedAsync();
            await Context.Database.MigrateAsync();

            // Act
            var tables = await Context.Database.SqlQuery<string>($@"
                SELECT name 
                FROM sqlite_master 
                WHERE type='table' 
                AND name NOT LIKE 'sqlite_%' 
                AND name NOT LIKE 'ef_%'
            ").ToListAsync();

            // Assert
            tables.Should().Contain("Tasks");
        }
    }
}