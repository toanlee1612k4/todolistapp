// File: Services/Interfaces/ITaskService.cs
using ToDoListApp.Api.DTOs.Task;

namespace ToDoListApp.Api.Services.Interfaces
{
    public interface ITaskService
    {
        // Lấy tất cả công việc của 1 dự án
        Task<IEnumerable<TaskDto>> GetTasksByProjectIdAsync(int projectId);

        // Tạo công việc mới
        // (Cần biết ai là người giao (reporterId)
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createDto, int reporterId);
        Task<TaskDto> GetTaskByIdAsync(int taskId);
        Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskDto updateDto);
    }
}