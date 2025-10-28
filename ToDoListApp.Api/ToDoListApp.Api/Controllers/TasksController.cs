
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Security.Claims;
using ToDoListApp.Api.Data; 
using ToDoListApp.Api.DTOs.Task;
using ToDoListApp.Api.Services.Interfaces;

namespace ToDoListApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc đăng nhập
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ApplicationDbContext _context; // Giữ nguyên khai báo

        // ===== SỬA HÀM KHỞI TẠO ĐỂ NHẬN DbContext =====
        public TasksController(ITaskService taskService, ApplicationDbContext context) // Thêm context vào đây
        {
            _taskService = taskService;
            _context = context; // Gán context đã nhận
        }
        // ===============================================

        // GET: api/tasks/byproject/{projectId:int}
        [HttpGet("byproject/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByProject(int projectId)
        {
            var tasks = await _taskService.GetTasksByProjectIdAsync(projectId);
            return Ok(tasks);
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reporterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var newTask = await _taskService.CreateTaskAsync(createDto, reporterId);

            // Sửa lại CreatedAtAction để trỏ đến GetTaskById
            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.TaskId }, newTask);
        }

        // GET: api/tasks/{id:int}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskDto>> GetTaskById(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc" });
            }
            return Ok(task);
        }

        // PUT: api/tasks/{id:int}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedTask = await _taskService.UpdateTaskAsync(id, updateDto);
            if (updatedTask == null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc" });
            }

            return Ok(updatedTask);
        }

        // GET: api/tasks/my
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetMyTasks()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var myTasks = await _context.Tasks
                .Where(t => t.AssigneeId == userId)
                .Include(t => t.Reporter)
                .Include(t => t.Assignee) // Mặc dù là userId, Include để load FullName
                .Include(t => t.Project)  // Phải Include Project để lấy được ProjectName
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt,
                    DueDate = t.DueDate,
                    ProjectId = t.ProjectId ?? 0,
                    // ===== BỎ COMMENT VÀ ĐẢM BẢO GÁN ĐÚNG =====
                    ProjectName = t.Project != null ? t.Project.ProjectName : "Việc cá nhân", 
                    // ==========================================
                    ReporterId = t.ReporterId,
                    ReporterName = t.Reporter.FullName,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee.FullName, // User này chắc chắn tồn tại (là chính userId)
                    IsRecurring = t.IsRecurring,
                    RecurrenceRule = t.RecurrenceRule
                })
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(myTasks);
        }
    }
}