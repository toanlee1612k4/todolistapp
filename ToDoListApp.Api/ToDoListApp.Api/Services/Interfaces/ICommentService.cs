// File: Services/Interfaces/ICommentService.cs
using System.Collections.Generic; // Cần cho IEnumerable
using System.Threading.Tasks; // Cần cho Task
using ToDoListApp.Api.DTOs.Comment; // Cần DTOs

namespace ToDoListApp.Api.Services.Interfaces
{
    public interface ICommentService
    {
        // Lấy tất cả bình luận của một công việc (Task)
        Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(int taskId);

        // Tạo một bình luận mới cho một công việc
        // Cần biết taskId, nội dung bình luận (createDto), và userId của người tạo
        Task<CommentDto> CreateCommentAsync(int taskId, CreateCommentDto createDto, int userId);

        // TODO: Có thể thêm các hàm khác sau này (Sửa, Xóa bình luận)
        Task<CommentDto> UpdateCommentAsync(int commentId, string content, int userId);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
    }
}
