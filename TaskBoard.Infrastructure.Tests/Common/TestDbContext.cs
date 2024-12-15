using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Infrastructure.Persistence;

namespace TaskBoard.Infrastructure.Tests.Common
{
    public abstract class TestDbContext : IDisposable
    {
        private readonly DbConnection _connection;
        protected readonly TaskBoardDbContext Context;

        protected TestDbContext()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<TaskBoardDbContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new TaskBoardDbContext(options);
            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();
        }

        protected DbContextOptions<TaskBoardDbContext> GetContextOptions()
        {
            return new DbContextOptionsBuilder<TaskBoardDbContext>()
                .UseSqlite(_connection)
                .Options;
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
            _connection.Dispose();
        }
    }
}