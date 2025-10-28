// File: DTOs/Department/DepartmentDto.cs
namespace ToDoListApp.Api.DTOs.Department
{
    // Dùng để Gửi và Nhận thông tin phòng ban
    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
    }
}