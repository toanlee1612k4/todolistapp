// File: DTOs/Comment/CreateCommentDto.cs
using System.ComponentModel.DataAnnotations;

namespace ToDoListApp.Api.DTOs.Comment
{
    // Dùng để nhận nội dung khi tạo bình luận mới
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "Nội dung bình luận không được để trống")]
        public string Content { get; set; }
    }
}
