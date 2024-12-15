using System.ComponentModel.DataAnnotations;
using TaskBoard.Domain.Enums;

namespace TaskBoard.BlazorServer.ViewModels
{
    public class TaskViewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Le titre est requis")]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskState Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}
