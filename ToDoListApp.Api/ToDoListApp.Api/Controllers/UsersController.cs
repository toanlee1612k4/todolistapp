// File: Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Cần dùng UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListApp.Api.DTOs.User;
using ToDoListApp.Api.Models; // Cần dùng User model

namespace ToDoListApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Chỉ người đã đăng nhập mới được lấy danh sách users
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/users
        // Lấy danh sách (ID, FullName, UserName, Email) của tất cả người dùng
        // TODO: Sau này có thể lọc theo dự án/phòng ban
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userManager.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    UserName = u.UserName,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
