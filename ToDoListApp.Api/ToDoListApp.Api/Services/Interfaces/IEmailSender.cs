// File: Services/Interfaces/IEmailSender.cs
namespace ToDoListApp.Api.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
