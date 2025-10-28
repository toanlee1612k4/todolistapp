// File: Services/DepartmentService.cs
using Microsoft.EntityFrameworkCore;
using ToDoListApp.Api.Data;
using ToDoListApp.Api.DTOs.Department;
using ToDoListApp.Api.Models;
using ToDoListApp.Api.Services.Interfaces;

namespace ToDoListApp.Api.Services
{
    // Class này "thi hành" hợp đồng IDepartmentService
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;

        // Yêu cầu DbContext để làm việc với database
        public DepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Thi hành hàm GetAllDepartmentsAsync
        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _context.Departments
                .Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description
                })
                .ToListAsync();

            return departments;
        }

        // Thi hành hàm CreateDepartmentAsync
        public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto createDto)
        {
            var newDepartment = new Department
            {
                DepartmentName = createDto.DepartmentName,
                Description = createDto.Description
            };

            _context.Departments.Add(newDepartment);
            await _context.SaveChangesAsync();

            // Chuyển đổi lại sang DTO để trả về
            var departmentDto = new DepartmentDto
            {
                DepartmentId = newDepartment.DepartmentId,
                DepartmentName = newDepartment.DepartmentName,
                Description = newDepartment.Description
            };

            return departmentDto;
        }
    }
}