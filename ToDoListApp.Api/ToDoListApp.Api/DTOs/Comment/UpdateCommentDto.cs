// File: DTOs/Comment/UpdateCommentDto.cs
using System.ComponentModel.DataAnnotations;

namespace ToDoListApp.Api.DTOs.Comment
{
    public class UpdateCommentDto
    {
        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }
    }
}
