// File: Services/ProjectMemberService.cs
using Microsoft.AspNetCore.Identity; // Cần UserManager
using Microsoft.EntityFrameworkCore;
using ToDoListApp.Api.Data;
using ToDoListApp.Api.DTOs.ProjectMember;
using ToDoListApp.Api.Models;
using ToDoListApp.Api.Services.Interfaces;

namespace ToDoListApp.Api.Services
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProjectMemberService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<MemberDto>> GetProjectMembersAsync(int projectId)
        {
            // Lấy danh sách UserId từ bảng ProjectMembers
            var memberIds = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Select(pm => pm.UserId)
                .ToListAsync();

            if (!memberIds.Any())
            {
                return Enumerable.Empty<MemberDto>();
            }

            // Lấy thông tin Users tương ứng
            var members = await _userManager.Users
                .Where(u => memberIds.Contains(u.Id))
                .Select(u => new MemberDto
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    UserName = u.UserName,
                    Email = u.Email
                })
                .ToListAsync();

            return members;
        }

        public async Task<MemberDto> AddMemberToProjectAsync(int projectId, AddMemberDto addMemberDto)
        {
            // 1. Tìm User cần thêm (ưu tiên Email)
            User userToAdd = null;
            if (!string.IsNullOrEmpty(addMemberDto.Email))
            {
                userToAdd = await _userManager.FindByEmailAsync(addMemberDto.Email);
            }
            else if (addMemberDto.UserId.HasValue)
            {
                userToAdd = await _userManager.FindByIdAsync(addMemberDto.UserId.Value.ToString());
            }

            if (userToAdd == null)
            {
                throw new KeyNotFoundException("Không tìm thấy người dùng.");
            }

            // 2. Kiểm tra Project có tồn tại không
            var projectExists = await _context.Projects.AnyAsync(p => p.ProjectId == projectId);
            if (!projectExists)
            {
                throw new KeyNotFoundException("Không tìm thấy dự án.");
            }

            // 3. Kiểm tra User đã là thành viên chưa
            var isAlreadyMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userToAdd.Id);
            if (isAlreadyMember)
            {
                // Có thể trả về thông tin user đã có hoặc ném lỗi tùy logic
                return new MemberDto { /* ... trả về thông tin userToAdd ... */ };
                // throw new InvalidOperationException("Người dùng đã là thành viên của dự án.");
            }

            // 4. Thêm vào bảng ProjectMembers
            var newMember = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userToAdd.Id
            };
            _context.ProjectMembers.Add(newMember);
            await _context.SaveChangesAsync();

            // 5. Trả về thông tin thành viên vừa thêm
            return new MemberDto
            {
                UserId = userToAdd.Id,
                FullName = userToAdd.FullName,
                UserName = userToAdd.UserName,
                Email = userToAdd.Email
            };
        }
        public async Task<bool> RemoveMemberFromProjectAsync(int projectId, int userId)
        {
            // Tìm bản ghi ProjectMember tương ứng
            var memberToRemove = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (memberToRemove == null)
            {
                // Không tìm thấy thành viên để xóa
                return false;
                // Hoặc throw new KeyNotFoundException("Thành viên không tồn tại trong dự án.");
            }

            // TODO: Sau này có thể thêm kiểm tra xem có phải là thành viên cuối cùng không,
            // hoặc có phải là người tạo dự án không (không cho xóa chính mình nếu là người tạo).

            // Xóa bản ghi khỏi DbContext
            _context.ProjectMembers.Remove(memberToRemove);
            // Lưu thay đổi vào database
            int affectedRows = await _context.SaveChangesAsync();

            // Trả về true nếu có ít nhất 1 dòng bị xóa
            return affectedRows > 0;
        }
    }
}
