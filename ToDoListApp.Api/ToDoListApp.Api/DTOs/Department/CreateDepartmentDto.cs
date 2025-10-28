// File: DTOs/Department/CreateDepartmentDto.cs
using System.ComponentModel.DataAnnotations;

namespace ToDoListApp.Api.DTOs.Department
{
    // Chỉ dùng để Nhận dữ liệu khi tạo mới
    public class CreateDepartmentDto
    {
        [Required(ErrorMessage = "Tên phòng ban là bắt buộc")]
        [StringLength(100)]
        public string DepartmentName { get; set; }

        public string Description { get; set; }
    }
}