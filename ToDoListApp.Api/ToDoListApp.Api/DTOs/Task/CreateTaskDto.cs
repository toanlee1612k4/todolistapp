// File: DTOs/Task/CreateTaskDto.cs
using System.ComponentModel.DataAnnotations;

namespace ToDoListApp.Api.DTOs.Task
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Tên công việc là bắt buộc")]
        [StringLength(200)]
        public string TaskName { get; set; }

        public string? Description { get; set; } // Cho phép null

        // ===== ĐÂY LÀ DÒNG QUAN TRỌNG NHẤT =====
        // Nó phải là "int?" (cho phép null)
        public int? ProjectId { get; set; }
        // ===================================

        public int Status { get; set; } = 0;
        public int Priority { get; set; } = 1;
        public DateTime? DueDate { get; set; }
        public int? AssigneeId { get; set; }
        public bool IsRecurring { get; set; } = false;
        public string? RecurrenceRule { get; set; } // Cũng phải là "string?"
    }
}