// File: DTOs/Project/CreateProjectDto.cs
using System.ComponentModel.DataAnnotations;

namespace ToDoListApp.Api.DTOs.Project
{
    // Dùng để Nhận thông tin khi tạo dự án
    public class CreateProjectDto
    {
        [Required(ErrorMessage = "Tên dự án là bắt buộc")]
        [StringLength(100)]
        public string ProjectName { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // ID của phòng ban (có thể null nếu dự án không thuộc PB nào)
        public int? DepartmentId { get; set; }
    }
}