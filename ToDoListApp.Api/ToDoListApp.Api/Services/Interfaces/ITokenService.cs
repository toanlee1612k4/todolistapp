using ToDoListApp.Api.Models;

namespace ToDoListApp.Api.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}