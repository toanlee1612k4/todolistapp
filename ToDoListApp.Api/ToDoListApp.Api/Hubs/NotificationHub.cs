// File: Hubs/NotificationHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ToDoListApp.Api.Hubs
{
    [Authorize] // Yêu cầu user phải đăng nhập (gửi token) mới kết nối được Hub
    public class NotificationHub : Hub
    {
        // Hàm này tự động chạy khi client (Flutter) kết nối
        public override async Task OnConnectedAsync()
        {
            // Lấy UserId từ Token của client
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                // Thêm user này vào một "Nhóm" (Group) có tên là chính UserId của họ
                // Việc này cho phép chúng ta gửi tin nhắn chỉ cho user đó
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"SignalR Client Connected: {Context.ConnectionId}, UserId: {userId}");
            }

            await base.OnConnectedAsync();
        }

        // Hàm này tự động chạy khi client ngắt kết nối
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Xóa khỏi nhóm
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"SignalR Client Disconnected: {Context.ConnectionId}, UserId: {userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        // (Tùy chọn) Hàm để test: Client gọi "SendMessage", Server gửi lại "ReceiveMessage"
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
