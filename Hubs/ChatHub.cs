using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using ChatApp.Backend.Models;
using ChatBackend.Data;
using Microsoft.Extensions.Logging;
using System;

namespace ChatApp.Backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MessagingContext _context;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(MessagingContext context, ILogger<ChatHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Yardımcı metot: Veritabanına MessageLog kaydı ekler.
        private async Task LogMessageAsync(string level, string message)
        {
            try
            {
                var messageLog = new MessageLog
                {
                    Level = level,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };
                _context.MessageLogs.Add(messageLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging message to database.");
            }
        }

        // Tüm client'lara mesaj gönderir.
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
            await LogMessageAsync("Info", $"Broadcast message from {user}: {message}");
        }

        // Birebir mesaj gönderme.
        public async Task SendPrivateMessage(string receiverConnectionId, string message)
        {
            var sender = Context.ConnectionId;
            await Clients.Client(receiverConnectionId).SendAsync("ReceivePrivateMessage", sender, message);
            await LogMessageAsync("Info", $"Private message from {sender} to {receiverConnectionId}: {message}");
        }

        // Grup mesajlaşması: Belirli bir gruba mesaj gönderir.
        public async Task SendMessageToGroup(string groupName, string message)
        {
            var sender = Context.ConnectionId;
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", sender, message);
            await LogMessageAsync("Info", $"Group message in '{groupName}' from {sender}: {message}");
        }

        // Kullanıcının belirtilen gruba katılmasını sağlar.
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            string logMsg = $"{Context.ConnectionId} joined the group '{groupName}'.";
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", logMsg);
            await LogMessageAsync("Info", logMsg);
        }

        // Kullanıcının belirtilen gruptan ayrılmasını sağlar.
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            string logMsg = $"{Context.ConnectionId} left the group '{groupName}'.";
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", logMsg);
            await LogMessageAsync("Info", logMsg);
        }
    }
}