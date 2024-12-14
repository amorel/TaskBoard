using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Domain.Interfaces
{
    /// <summary>
    /// Interface for board task repository operations
    /// </summary>
    public interface ITaskRepository
    {
        /// <summary>
        /// Gets all board tasks
        /// </summary>
        Task<IEnumerable<BoardTask>> GetAllAsync();

        /// <summary>
        /// Gets a board task by its ID
        /// </summary>
        Task<BoardTask?> GetByIdAsync(Guid id);

        /// <summary>
        /// Adds a new board task
        /// </summary>
        Task<BoardTask> AddAsync(BoardTask boardTask);

        /// <summary>
        /// Updates an existing board task
        /// </summary>
        Task<BoardTask> UpdateAsync(BoardTask boardTask);

        /// <summary>
        /// Deletes a board task
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Gets board tasks by their current state
        /// </summary>
        Task<IEnumerable<BoardTask>> GetByStateAsync(TaskState state);
    }
}