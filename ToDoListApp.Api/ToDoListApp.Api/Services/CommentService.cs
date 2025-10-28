// File: Services/CommentService.cs
using Microsoft.AspNetCore.Identity; // Cần UserManager
using Microsoft.EntityFrameworkCore; // Cần DbContext và Include/Select/ToListAsync
using System; // Cần DateTime
using System.Collections.Generic; // Cần IEnumerable/List
using System.Linq; // Cần Where/Select/OrderByDescending
using System.Threading.Tasks; // Cần Task/async/await
using ToDoListApp.Api.Data;
using ToDoListApp.Api.DTOs.Comment;
using ToDoListApp.Api.Models; // Cần Comment và User models
using ToDoListApp.Api.Services.Interfaces;

namespace ToDoListApp.Api.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager; // Để lấy thông tin User

        public CommentService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Lấy bình luận theo Task ID
        public async Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(int taskId)
        {
            var comments = await _context.Comments
                .Where(c => c.TaskId == taskId) // Lọc theo TaskId
                .Include(c => c.User) // Lấy kèm thông tin User (người bình luận)
                .OrderByDescending(c => c.CreatedAt) // Sắp xếp mới nhất lên đầu
                .Select(c => new CommentDto // Chuyển đổi sang DTO
                {
                    CommentId = c.CommentId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    TaskId = c.TaskId,
                    UserId = c.UserId,
                    UserFullName = c.User.FullName, // Lấy FullName từ User đã Include
                    UserName = c.User.UserName
                })
                .ToListAsync();

            return comments;
        }

        // Tạo bình luận mới
        public async Task<CommentDto> CreateCommentAsync(int taskId, CreateCommentDto createDto, int userId)
        {
            // Kiểm tra Task có tồn tại không (tùy chọn, có thể bỏ qua nếu DB có foreign key constraint)
            var taskExists = await _context.Tasks.AnyAsync(t => t.TaskId == taskId);
            if (!taskExists)
            {
                throw new KeyNotFoundException($"Không tìm thấy công việc với ID: {taskId}");
            }

            // Lấy thông tin User tạo bình luận (để trả về DTO)
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {userId}"); // Lỗi lạ nếu xảy ra
            }

            // Tạo đối tượng Comment mới
            var newComment = new Comment
            {
                Content = createDto.Content,
                TaskId = taskId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow // Luôn dùng UTC cho thời gian trên server
            };

            // Thêm vào DbContext và Lưu
            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            // Chuyển đổi Comment vừa tạo sang DTO để trả về
            return new CommentDto
            {
                CommentId = newComment.CommentId,
                Content = newComment.Content,
                CreatedAt = newComment.CreatedAt,
                TaskId = newComment.TaskId,
                UserId = newComment.UserId,
                UserFullName = user.FullName, // Lấy từ user đã load
                UserName = user.UserName
            };
        }
        public async Task<CommentDto> UpdateCommentAsync(int commentId, string content, int userId)
        {
            // Tìm comment cần sửa
            var commentToUpdate = await _context.Comments
                                        .Include(c => c.User) // Include User để trả về DTO
                                        .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (commentToUpdate == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy bình luận với ID: {commentId}");
            }

            // Kiểm tra quyền: Chỉ người tạo mới được sửa
            if (commentToUpdate.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền sửa bình luận này.");
            }

            // Cập nhật nội dung
            commentToUpdate.Content = content;
            // Có thể thêm logic cập nhật thời gian "EditedAt" nếu muốn

            await _context.SaveChangesAsync();

            // Trả về DTO đã cập nhật
            return new CommentDto
            {
                CommentId = commentToUpdate.CommentId,
                Content = commentToUpdate.Content,
                CreatedAt = commentToUpdate.CreatedAt,
                TaskId = commentToUpdate.TaskId,
                UserId = commentToUpdate.UserId,
                UserFullName = commentToUpdate.User.FullName,
                UserName = commentToUpdate.User.UserName
            };
        }
        // ==========================

        // ===== THÊM HÀM DELETE =====
        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            // Tìm comment cần xóa
            var commentToDelete = await _context.Comments
                                        .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (commentToDelete == null)
            {
                // Không tìm thấy để xóa, trả về false (hoặc ném lỗi tùy logic Controller)
                return false;
            }

            // Kiểm tra quyền: Chỉ người tạo mới được xóa
            if (commentToDelete.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa bình luận này.");
            }

            // Xóa comment
            _context.Comments.Remove(commentToDelete);
            int affectedRows = await _context.SaveChangesAsync();

            // Trả về true nếu xóa thành công
            return affectedRows > 0;
        }
    }
}
