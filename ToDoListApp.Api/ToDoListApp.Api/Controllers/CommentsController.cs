// File: Controllers/CommentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ToDoListApp.Api.DTOs.Comment; // <-- Đảm bảo using DTOs
using ToDoListApp.Api.Services.Interfaces;

namespace ToDoListApp.Api.Controllers
{
    [Route("api/tasks/{taskId:int}/[controller]")] // Route lồng nhau
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: api/tasks/5/comments (Lấy bình luận)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int taskId)
        {
            try
            {
                var comments = await _commentService.GetCommentsByTaskIdAsync(taskId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting comments for task {taskId}: {ex.Message}");
                return StatusCode(500, "Đã xảy ra lỗi khi tải bình luận.");
            }
        }

        // POST: api/tasks/5/comments (Tạo bình luận)
        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment(int taskId, [FromBody] CreateCommentDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Không thể xác định người dùng.");

            try
            {
                var newComment = await _commentService.CreateCommentAsync(taskId, createDto, userId);
                return Ok(newComment); // Trả về 200 OK
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating comment for task {taskId}: {ex.Message}");
                return StatusCode(500, "Đã xảy ra lỗi khi tạo bình luận.");
            }
        }

        // ===== THÊM API PUT (UPDATE) =====
        // PUT: api/tasks/5/comments/123 (Sửa comment 123 của task 5)
        [HttpPut("{id:int}")] // id ở đây là commentId
        public async Task<ActionResult<CommentDto>> UpdateComment(int taskId, int id, [FromBody] UpdateCommentDto updateDto) // <-- Thêm DTO UpdateCommentDto
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Lấy UserId của người đang thực hiện
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Không thể xác định người dùng.");

            try
            {
                // Gọi service để cập nhật (truyền commentId 'id', nội dung mới, và userId để kiểm tra quyền)
                var updatedComment = await _commentService.UpdateCommentAsync(id, updateDto.Content, userId);
                return Ok(updatedComment); // Trả về comment đã cập nhật
            }
            catch (KeyNotFoundException ex)
            {
                // Lỗi không tìm thấy comment
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Lỗi không có quyền sửa
                return Forbid(); // Trả về lỗi 403 Forbidden
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating comment {id} for task {taskId}: {ex.Message}");
                return StatusCode(500, "Lỗi khi cập nhật bình luận.");
            }
        }
        // ================================

        // ===== THÊM API DELETE =====
        // DELETE: api/tasks/5/comments/123 (Xóa comment 123 của task 5)
        [HttpDelete("{id:int}")] // id là commentId
        public async Task<IActionResult> DeleteComment(int taskId, int id)
        {
            // Lấy UserId của người đang thực hiện
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Không thể xác định người dùng.");

            try
            {
                // Gọi service để xóa (truyền commentId 'id' và userId để kiểm tra quyền)
                bool success = await _commentService.DeleteCommentAsync(id, userId);
                if (success)
                {
                    return NoContent(); // 204 No Content - Xóa thành công
                }
                else
                {
                    // Service trả về false nghĩa là không tìm thấy comment
                    return NotFound(new { Message = "Không tìm thấy bình luận để xóa." });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Lỗi không có quyền xóa
                return Forbid(); // Trả về lỗi 403 Forbidden
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting comment {id} for task {taskId}: {ex.Message}");
                return StatusCode(500, "Lỗi khi xóa bình luận.");
            }
        }
        // ==========================
    }
}