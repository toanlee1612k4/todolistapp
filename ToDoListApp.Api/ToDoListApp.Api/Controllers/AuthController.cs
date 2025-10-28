
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities; 
using System.Security.Claims;
using System.Text;                   
using System.Text.Encodings.Web;    
using ToDoListApp.Api.DTOs.Auth;
using ToDoListApp.Api.Models;
using ToDoListApp.Api.Services.Interfaces;


namespace ToDoListApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender; 

        
        public AuthController(
            UserManager<User> userManager,
            ITokenService tokenService,
            IEmailSender emailSender) 
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailSender = emailSender; 
        }
        

        
        [HttpPost("register")]
        [AllowAnonymous] 
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null) { return BadRequest(new { Message = "Email đã được sử dụng." }); }
            var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingUserByUsername != null) { return BadRequest(new { Message = "Tên tài khoản đã tồn tại." }); }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Username,
                PhoneNumber = model.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) { return BadRequest(result.Errors); }
            return Ok(new { Message = "Đăng ký thành công!" });
        }

        
        [HttpPost("login")]
        [AllowAnonymous] 
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { Message = "Tên tài khoản hoặc mật khẩu không đúng." });
            }
            var token = _tokenService.CreateToken(user);
            
            return Ok(new
            {
                Message = "Đăng nhập thành công!",
                Token = token,
                Username = user.UserName,
                Email = user.Email,
                FullName = user.FullName
            });
        }

        // --- API Đổi mật khẩu (cho người đang đăng nhập) ---
        [HttpPost("change-password")]
        [Authorize] // <-- Yêu cầu đăng nhập
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) { return Unauthorized(); }
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded) { return BadRequest(result.Errors); }
            return Ok(new { Message = "Đổi mật khẩu thành công!" });
        }

        // ===== THÊM API FORGOT PASSWORD =====
        [HttpPost("forgot-password")]
        [AllowAnonymous] // Không cần đăng nhập
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Luôn trả về OK để bảo mật
                return Ok(new { Message = "Nếu email của bạn tồn tại, một email đặt lại mật khẩu đã được gửi." });
            }

            // Tạo Reset Token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // *** QUAN TRỌNG: Thay đổi URL Front-end của bạn ở đây ***
            var resetLink = $"https://your-frontend-domain/reset-password?email={UrlEncoder.Default.Encode(user.Email)}&token={encodedToken}";

            // Gửi email thật (dùng IEmailSender đã inject)
            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Yêu cầu đặt lại mật khẩu",
                    $"Nhấn vào link sau để đặt lại mật khẩu: <a href='{resetLink}'>Đặt lại mật khẩu</a>. Link có hiệu lực trong thời gian ngắn."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR sending password reset email to {user.Email}: {ex.Message}");
                // Vẫn trả về OK cho người dùng
                return Ok(new { Message = "Yêu cầu đã được xử lý. Vui lòng kiểm tra email (kể cả thư mục spam)." });
            }

            return Ok(new { Message = "Nếu email của bạn tồn tại, một email đặt lại mật khẩu đã được gửi." });
        }
        // =====================================

        // ===== THÊM API RESET PASSWORD =====
        [HttpPost("reset-password")]
        [AllowAnonymous] // Không cần đăng nhập
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new { Message = "Yêu cầu đặt lại mật khẩu không hợp lệ." });
            }

            // Giải mã token
            string decodedToken;
            try
            {
                byte[] tokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                decodedToken = Encoding.UTF8.GetString(tokenBytes);
            }
            catch (FormatException)
            {
                return BadRequest(new { Message = "Mã đặt lại mật khẩu không hợp lệ." });
            }

            // Thực hiện đặt lại
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
            {
                // Lọc lỗi trả về
                List<string> errors = new List<string>();
                foreach (var error in result.Errors)
                {
                    if (error.Code == "InvalidToken") { errors.Add("Yêu cầu đặt lại mật khẩu không hợp lệ hoặc đã hết hạn."); }
                    else { errors.Add(error.Description); }
                }
                return BadRequest(new { Errors = errors });
            }

            return Ok(new { Message = "Đặt lại mật khẩu thành công!" });
        }
        // ==================================
    }
}