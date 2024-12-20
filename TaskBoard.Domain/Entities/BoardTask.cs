using TaskBoard.Domain.Enums;

namespace TaskBoard.Domain.Entities
{
    /// <summary>
    /// Represents a task in the system
    /// </summary>
    public class BoardTask
    {
        public BoardTask()
        {
            Id = Guid.NewGuid(); 
            CreatedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
            Status = TaskState.Todo; 
        }

        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskState Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}