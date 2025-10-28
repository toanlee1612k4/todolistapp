// File: Services/Interfaces/IProjectService.cs
using ToDoListApp.Api.DTOs.Project;

namespace ToDoListApp.Api.Services.Interfaces
{
    public interface IProjectService
    {
        // Lấy tất cả dự án
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();

        // Tạo dự án mới
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto createDto);
    }
}