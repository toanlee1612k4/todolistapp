// File: Controllers/ProjectMembersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoListApp.Api.DTOs.ProjectMember;
using ToDoListApp.Api.Services.Interfaces;

namespace ToDoListApp.Api.Controllers
{
    [Route("api/projects/{projectId:int}/members")] // Route lồng nhau
    [ApiController]
    [Authorize]
    public class ProjectMembersController : ControllerBase
    {
        private readonly IProjectMemberService _memberService;

        public ProjectMembersController(IProjectMemberService memberService)
        {
            _memberService = memberService;
        }

        // GET: api/projects/5/members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers(int projectId)
        {
            // TODO: Thêm kiểm tra quyền xem dự án sau này
            var members = await _memberService.GetProjectMembersAsync(projectId);
            return Ok(members);
        }

        // POST: api/projects/5/members
        [HttpPost]
        public async Task<ActionResult<MemberDto>> AddMember(int projectId, [FromBody] AddMemberDto addMemberDto)
        {
            // TODO: Thêm kiểm tra quyền thêm thành viên sau này
            try
            {
                var newMember = await _memberService.AddMemberToProjectAsync(projectId, addMemberDto);
                // Trả về thông tin thành viên vừa thêm
                return Ok(newMember); // Trả về 200 OK thay vì 201 Created vì thành viên đã tồn tại
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // Bắt lỗi nếu user đã là thành viên
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex) // Bắt các lỗi khác
            {
                // Log lỗi ra console/file log
                return StatusCode(500, "Đã xảy ra lỗi khi thêm thành viên.");
            }
        }
        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> RemoveMember(int projectId, int userId)
        {
            // TODO: Thêm kiểm tra quyền xóa thành viên sau này

            try
            {
                bool success = await _memberService.RemoveMemberFromProjectAsync(projectId, userId);
                if (success)
                {
                    // Trả về 204 No Content nếu xóa thành công
                    return NoContent();
                }
                else
                {
                    // Không tìm thấy thành viên để xóa
                    return NotFound(new { Message = "Thành viên không tồn tại trong dự án." });
                }
            }
            // catch (KeyNotFoundException ex) // Bắt lỗi nếu service ném lỗi
            // {
            //     return NotFound(new { Message = ex.Message });
            // }
            catch (Exception ex) // Bắt các lỗi khác
            {
                // Log lỗi
                return StatusCode(500, "Đã xảy ra lỗi khi xóa thành viên.");
            }
        }
    }
}