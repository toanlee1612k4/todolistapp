// File: Services/ProjectService.cs
using Microsoft.EntityFrameworkCore;
using ToDoListApp.Api.Data;
using ToDoListApp.Api.DTOs.Project;
using ToDoListApp.Api.Models; // Import Models
using ToDoListApp.Api.Services.Interfaces;

namespace ToDoListApp.Api.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            var projects = await _context.Projects
                // Include để lấy cả thông tin Department liên quan
                .Include(p => p.Department)
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    DepartmentId = p.DepartmentId,
                    // Lấy tên phòng ban từ bảng Department
                    DepartmentName = p.Department != null ? p.Department.DepartmentName : null
                })
                .ToListAsync();

            return projects;
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createDto)
        {
            var newProject = new Project
            {
                ProjectName = createDto.ProjectName,
                Description = createDto.Description,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                DepartmentId = createDto.DepartmentId
            };

            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();

            // Load lại thông tin phòng ban (nếu có) để trả về DTO
            if (newProject.DepartmentId != null)
            {
                newProject.Department = await _context.Departments
                    .FindAsync(newProject.DepartmentId);
            }

            // Chuyển đổi sang DTO
            var projectDto = new ProjectDto
            {
                ProjectId = newProject.ProjectId,
                ProjectName = newProject.ProjectName,
                Description = newProject.Description,
                StartDate = newProject.StartDate,
                EndDate = newProject.EndDate,
                DepartmentId = newProject.DepartmentId,
                DepartmentName = newProject.Department?.DepartmentName
            };

            return projectDto;
        }
    }
}