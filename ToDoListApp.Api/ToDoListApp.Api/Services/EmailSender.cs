// File: Services/EmailSender.cs
using MailKit.Net.Smtp; // Thêm using
using MailKit.Security; // Thêm using
using Microsoft.Extensions.Options; // Thêm using
using MimeKit; // Thêm using
using System.Threading.Tasks; // Thêm using
using ToDoListApp.Api.Services.Interfaces; // Thêm using

namespace ToDoListApp.Api.Services
{
    // Class để đọc cấu hình từ appsettings.json
    public class MailSettings
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; } // Đây là App Password
        public string Host { get; set; } = "smtp.gmail.com"; // SMTP của Gmail
        public int Port { get; set; } = 587; // Port TLS của Gmail
    }

    public class EmailSender : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        // Inject MailSettings vào constructor
        public EmailSender(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();

            // From (Người gửi)
            emailMessage.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            // To (Người nhận)
            emailMessage.To.Add(MailboxAddress.Parse(email));
            // Subject (Tiêu đề)
            emailMessage.Subject = subject;
            // Body (Nội dung HTML)
            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            // Dùng SmtpClient của MailKit
            using var smtp = new SmtpClient();
            try
            {
                // Kết nối đến server Gmail SMTP
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                // Xác thực bằng email và App Password
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
                // Gửi email
                await smtp.SendAsync(emailMessage);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi hoặc xử lý
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw; // Ném lại lỗi để API biết
            }
            finally
            {
                // Ngắt kết nối
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
