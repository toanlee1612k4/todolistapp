// File: Services/Interfaces/IDepartmentService.cs
using ToDoListApp.Api.DTOs.Department;


namespace ToDoListApp.Api.Services.Interfaces
{
    public interface IDepartmentService
    {
        // "Hợp đồng" nói rằng:
        // 1. Sẽ có 1 hàm lấy tất cả phòng ban
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();

        // 2. Sẽ có 1 hàm tạo phòng ban mới
        Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto createDto);
    }
}