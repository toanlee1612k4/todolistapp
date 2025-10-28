// File: Models/Comment.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoListApp.Api.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; } // Nội dung bình luận

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Lưu giờ UTC

        // Khóa ngoại đến Task
        [Required]
        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual Task Task { get; set; } // Quan hệ với Task

        // Khóa ngoại đến User (Người bình luận)
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } // Quan hệ với User
    }
}