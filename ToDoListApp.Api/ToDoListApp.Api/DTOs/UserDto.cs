// File: DTOs/User/UserDto.cs
namespace ToDoListApp.Api.DTOs.User
{
    // DTO đơn giản chỉ chứa thông tin cần thiết để hiển thị
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; } // Có thể thêm UserName
        public string Email { get; set; }    // Hoặc Email
    }
}