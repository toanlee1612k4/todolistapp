// File: Services/TaskService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToDoListApp.Api.Data;
using ToDoListApp.Api.DTOs.Task;
using ToDoListApp.Api.Models;
using ToDoListApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using ToDoListApp.Api.Hubs;
using Task = ToDoListApp.Api.Models.Task;


namespace ToDoListApp.Api.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TaskService(ApplicationDbContext context, UserManager<User> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByProjectIdAsync(int projectId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.Reporter)
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt,
                    DueDate = t.DueDate,
                    ProjectId = t.ProjectId ?? 0, // Dùng ?? 0
                    ProjectName = t.Project != null ? t.Project.ProjectName : null,
                    ReporterId = t.ReporterId,
                    ReporterName = t.Reporter.FullName,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee != null ? t.Assignee.FullName : "Chưa gán",
                    IsRecurring = t.IsRecurring,
                    RecurrenceRule = t.RecurrenceRule
                })
                .ToListAsync();
            return tasks;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createDto, int reporterId)
        {
            // Kiểm tra ProjectId nếu nó được cung cấp (KHÔNG phải null)
            if (createDto.ProjectId != null) // <-- Sửa: Dùng != null
            {
                var projectExists = await _context.Projects.AnyAsync(p => p.ProjectId == createDto.ProjectId.Value); // Dùng .Value
                if (!projectExists)
                {
                    throw new KeyNotFoundException($"Không tìm thấy dự án với ID: {createDto.ProjectId.Value}");
                }
            }

            var reporterUser = await _userManager.FindByIdAsync(reporterId.ToString());
            if (reporterUser == null) { throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {reporterId}"); }

            // Logic cho task cá nhân
            int? actualAssigneeId = createDto.AssigneeId;
            if (createDto.ProjectId == null) // <-- Sửa: Dùng == null
            {
                actualAssigneeId = reporterId; // Tự gán cho người tạo
            }

            var newTask = new Task
            {
                TaskName = createDto.TaskName,
                Description = createDto.Description,
                ProjectId = createDto.ProjectId, // Sẽ là null
                Status = createDto.Status,
                Priority = createDto.Priority,
                DueDate = createDto.DueDate,
                AssigneeId = actualAssigneeId, // Gán người tạo
                ReporterId = reporterId,
                CreatedAt = DateTime.UtcNow,
                IsRecurring = createDto.IsRecurring,
                RecurrenceRule = createDto.IsRecurring ? createDto.RecurrenceRule : null
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            // Load dữ liệu liên quan
            User? assigneeUser = null;
            if (newTask.AssigneeId != null)
            {
                assigneeUser = await _userManager.FindByIdAsync(newTask.AssigneeId.Value.ToString());
                newTask.Assignee = assigneeUser;
            }
            if (newTask.ProjectId != null)
            {
                await _context.Entry(newTask).Reference(t => t.Project).LoadAsync();
            }

            // Gửi thông báo real-time nếu task này được gán cho ai đó
            if (newTask.AssigneeId != null)
            {
                // Gửi tin nhắn đến "Nhóm" (Group) của user được gán
                await _hubContext.Clients.Group(newTask.AssigneeId.Value.ToString())
                    .SendAsync("ReceiveTaskUpdate", "Bạn vừa được gán một công việc mới!"); // Tên sự kiện: "ReceiveTaskUpdate"
            }
            // Trả về DTO
            return new TaskDto
            {
                TaskId = newTask.TaskId,
                TaskName = newTask.TaskName,
                Description = newTask.Description,
                Status = newTask.Status,
                Priority = newTask.Priority,
                CreatedAt = newTask.CreatedAt,
                DueDate = newTask.DueDate,
                ProjectId = newTask.ProjectId ?? 0, // Dùng ?? 0
                ProjectName = newTask.Project?.ProjectName ?? (newTask.ProjectId == null ? "Việc cá nhân" : null), // Xử lý null
                ReporterId = newTask.ReporterId,
                ReporterName = reporterUser.FullName,
                AssigneeId = newTask.AssigneeId,
                AssigneeName = assigneeUser?.FullName,
                IsRecurring = newTask.IsRecurring,
                RecurrenceRule = newTask.RecurrenceRule
            };
        }

        public async Task<TaskDto> GetTaskByIdAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Reporter)
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null) { return null; }

            return new TaskDto
            {
                TaskId = task.TaskId,
                TaskName = task.TaskName,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                ProjectId = task.ProjectId ?? 0, // Dùng ?? 0
                ProjectName = task.Project?.ProjectName ?? (task.ProjectId == null ? "Việc cá nhân" : null), // Xử lý null
                ReporterId = task.ReporterId,
                ReporterName = task.Reporter.FullName,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee?.FullName,
                IsRecurring = task.IsRecurring,
                RecurrenceRule = task.RecurrenceRule
            };
        }

        public async Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskDto updateDto)
        {
            var taskToUpdate = await _context.Tasks.FindAsync(taskId);
            if (taskToUpdate == null) { return null; }

            taskToUpdate.TaskName = updateDto.TaskName;
            taskToUpdate.Description = updateDto.Description;
            taskToUpdate.Status = updateDto.Status;
            taskToUpdate.Priority = updateDto.Priority;
            taskToUpdate.DueDate = updateDto.DueDate;
            taskToUpdate.AssigneeId = updateDto.AssigneeId;
            taskToUpdate.IsRecurring = updateDto.IsRecurring;
            taskToUpdate.RecurrenceRule = updateDto.IsRecurring ? updateDto.RecurrenceRule : null;

            await _context.SaveChangesAsync();

            // Load dữ liệu liên quan
            await _context.Entry(taskToUpdate).Reference(t => t.Reporter).LoadAsync();
            User? assigneeUser = null;
            if (taskToUpdate.AssigneeId != null)
            {
                assigneeUser = await _userManager.FindByIdAsync(taskToUpdate.AssigneeId.Value.ToString());
            }
            if (taskToUpdate.ProjectId != null)
            {
                await _context.Entry(taskToUpdate).Reference(t => t.Project).LoadAsync();
            }

            if (taskToUpdate.AssigneeId != null)
            {
                await _hubContext.Clients.Group(taskToUpdate.AssigneeId.Value.ToString())
                    .SendAsync("ReceiveTaskUpdate", $"Công việc '{taskToUpdate.TaskName}' vừa được cập nhật."); // Tên sự kiện: "ReceiveTaskUpdate"
            }

            // Trả về DTO
            return new TaskDto
            {
                TaskId = taskToUpdate.TaskId,
                TaskName = taskToUpdate.TaskName,
                Description = taskToUpdate.Description,
                Status = taskToUpdate.Status,
                Priority = taskToUpdate.Priority,
                CreatedAt = taskToUpdate.CreatedAt,
                DueDate = taskToUpdate.DueDate,
                ProjectId = taskToUpdate.ProjectId ?? 0, // Dùng ?? 0
                ProjectName = taskToUpdate.Project?.ProjectName ?? (taskToUpdate.ProjectId == null ? "Việc cá nhân" : null), // Xử lý null
                ReporterId = taskToUpdate.ReporterId,
                ReporterName = taskToUpdate.Reporter.FullName,
                AssigneeId = taskToUpdate.AssigneeId,
                AssigneeName = assigneeUser?.FullName,
                IsRecurring = taskToUpdate.IsRecurring,
                RecurrenceRule = taskToUpdate.RecurrenceRule
            };
        }
    }
}