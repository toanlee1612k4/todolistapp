
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoListApp.Api.Models
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        [StringLength(200)]
        public string TaskName { get; set; }

        public string? Description { get; set; }

        // 0: ToDo, 1: InProgress, 2: Done (Bạn có thể dùng Enum sau)
        public int Status { get; set; } = 0;

        // 0: Low, 1: Medium, 2: High
        public int Priority { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; } // Ngày hết hạn

        // Khóa ngoại liên kết với Dự án
        public int? ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }

        // Khóa ngoại liên kết với Người được giao (User)
        public int? AssigneeId { get; set; } // Người được giao
        [ForeignKey("AssigneeId")]
        public virtual User? Assignee { get; set; }

        // Khóa ngoại liên kết với Người giao (User)
        [Required]
        public int ReporterId { get; set; } // Người giao
        [ForeignKey("ReporterId")]
        public virtual User Reporter { get; set; }

        public bool IsRecurring { get; set; } = false; // Mặc định là không lặp lại
        public string? RecurrenceRule { get; set; } // Lưu quy tắc lặp (ví dụ: "DAILY", "WEEKLY:MON,FRI")
    }
}