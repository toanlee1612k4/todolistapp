// File: DTOs/ProjectMember/AddMemberDto.cs
using System.ComponentModel.DataAnnotations;

namespace ToDoListApp.Api.DTOs.ProjectMember
{
    // Dùng để nhận thông tin khi thêm thành viên
    public class AddMemberDto
    {
        // Có thể thêm bằng UserId hoặc Username/Email
        public int? UserId { get; set; }

        [EmailAddress]
        public string Email { get; set; } // Ưu tiên thêm bằng Email cho dễ
    }
}