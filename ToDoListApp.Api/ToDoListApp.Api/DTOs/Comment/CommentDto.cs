// File: DTOs/Comment/CommentDto.cs
using System;

namespace ToDoListApp.Api.DTOs.Comment
{
    // Dùng để gửi thông tin bình luận ra ngoài
    public class CommentDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TaskId { get; set; }

        // Thông tin người bình luận
        public int UserId { get; set; }
        public string UserFullName { get; set; } // Hiển thị tên đầy đủ
        public string UserName { get; set; } // Có thể thêm Username
    }
}