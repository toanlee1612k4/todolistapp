
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ToDoListApp.Api.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase
    {
        // GET: api/admin/admin-only
        [HttpGet("admin-only")] // Route con
        public IActionResult AdminOnlyData()
        {
            // Chỉ Admin mới gọi được API này
            return Ok(new { Message = "Chào mừng Admin! (Từ AdminController)" });
        }

    }
}
