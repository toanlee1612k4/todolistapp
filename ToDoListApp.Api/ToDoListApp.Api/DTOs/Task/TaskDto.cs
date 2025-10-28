// File: DTOs/Task/TaskDto.cs
namespace ToDoListApp.Api.DTOs.Task
{
    // Dùng để Gửi thông tin chi tiết của 1 công việc
    public class TaskDto
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }

        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }

        // Thông tin người giao (Reporter)
        public int ReporterId { get; set; }
        public string ReporterName { get; set; }

        // Thông tin người nhận (Assignee)
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrenceRule { get; set; }
    }
}
