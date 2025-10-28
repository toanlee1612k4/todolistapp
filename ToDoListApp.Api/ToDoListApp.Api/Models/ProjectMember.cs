// File: Models/ProjectMember.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoListApp.Api.Models
{
    // Bảng trung gian nối Users và Projects (Quan hệ nhiều-nhiều)
    public class ProjectMember
    {
        [Key]
        public int ProjectMemberId { get; set; }

        // Khóa ngoại đến Project
        [Required]
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        // Khóa ngoại đến User
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // Có thể thêm vai trò (Role) sau này, ví dụ: "Admin", "Member"
        // public string Role { get; set; } = "Member"; 
    }
}