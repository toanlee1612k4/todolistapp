using System.ComponentModel.DataAnnotations;

namespace ToDoListApp.Api.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required] public string FullName { get; set; }
        [Required][EmailAddress] public string Email { get; set; }
        [Required] public string PhoneNumber { get; set; }
        [Required] public string Username { get; set; }
        [Required][MinLength(6)] public string Password { get; set; }
        [Required][Compare("Password")] public string ConfirmPassword { get; set; }
    }
}