// File: DTOs/Task/UpdateTaskDto.cs
using System.ComponentModel.DataAnnotations;

namespace ToDoListApp.Api.DTOs.Task
{
    // Dùng để Nhận dữ liệu khi CẬP NHẬT
    public class UpdateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string TaskName { get; set; }

        public string Description { get; set; }

        [Range(0, 2)] // 0=ToDo, 1=InProgress, 2=Done
        public int Status { get; set; }

        [Range(0, 2)] // 0=Low, 1=Medium, 2=High
        public int Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public int? AssigneeId { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrenceRule { get; set; }
    }
}