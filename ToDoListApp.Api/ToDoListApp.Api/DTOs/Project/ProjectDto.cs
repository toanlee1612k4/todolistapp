// File: DTOs/Project/ProjectDto.cs
namespace ToDoListApp.Api.DTOs.Project
{
    // Dùng để Gửi thông tin chi tiết dự án
    public class ProjectDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Chúng ta có thể thêm thông tin phòng ban nếu cần
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } // Tên phòng ban
    }
}