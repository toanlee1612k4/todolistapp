using Microsoft.AspNetCore.Identity;

namespace ToDoListApp.Api.Models
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; }
    }
}