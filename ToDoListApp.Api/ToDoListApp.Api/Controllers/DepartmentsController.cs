// File: Controllers/DepartmentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoListApp.Api.DTOs.Department;
using ToDoListApp.Api.Services.Interfaces; // <-- Dùng Service Interface

namespace ToDoListApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        // Controller KHÔNG BIẾT _context
        // Nó chỉ biết Service
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: api/departments
        // Chỉ gọi Service và trả về kết quả
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }

        // POST: api/departments
        // Chỉ gọi Service và trả về kết quả
        [HttpPost]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto createDto)
        {
            // Controller không cần biết logic tạo, nó chỉ cần gọi hàm
            var newDepartment = await _departmentService.CreateDepartmentAsync(createDto);

            // Trả về 201 Created
            return CreatedAtAction(nameof(GetDepartments), new { id = newDepartment.DepartmentId }, newDepartment);
        }
    }
}
