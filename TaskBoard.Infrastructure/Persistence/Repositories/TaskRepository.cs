using Microsoft.EntityFrameworkCore;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;
using TaskBoard.Domain.Interfaces;

namespace TaskBoard.Infrastructure.Persistence.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskBoardDbContext _context;

        public TaskRepository(TaskBoardDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BoardTask>> GetAllAsync()
        {
            return await _context.Tasks.ToListAsync();
        }

        public async Task<BoardTask?> GetByIdAsync(Guid id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<BoardTask> AddAsync(BoardTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<BoardTask> UpdateAsync(BoardTask task)
        {
            _context.Entry(task).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task DeleteAsync(Guid id)
        {
            var task = await GetByIdAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BoardTask>> GetByStateAsync(TaskState state)
        {
            return await _context.Tasks
                .Where(t => t.Status == state)
                .ToListAsync();
        }
    }
}
