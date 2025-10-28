// File: Services/Interfaces/IProjectMemberService.cs
using ToDoListApp.Api.DTOs.ProjectMember;

namespace ToDoListApp.Api.Services.Interfaces
{
    public interface IProjectMemberService
    {
        Task<IEnumerable<MemberDto>> GetProjectMembersAsync(int projectId);
        Task<MemberDto> AddMemberToProjectAsync(int projectId, AddMemberDto addMemberDto);
        // Task<bool> RemoveMemberFromProjectAsync(int projectId, int userId); // Sẽ làm sau
        Task<bool> RemoveMemberFromProjectAsync(int projectId, int userId);
    }
}
