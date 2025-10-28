// File: DTOs/ProjectMember/MemberDto.cs
namespace ToDoListApp.Api.DTOs.ProjectMember
{
    // Dùng để hiển thị thông tin thành viên
    public class MemberDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        // public string Role { get; set; } // Sẽ thêm sau nếu cần vai trò
    }
}