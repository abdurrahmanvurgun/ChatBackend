using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
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
    // Map userId -> set of connectionIds (support multiple devices/tabs)
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, byte>> _connections = new();

        public ChatHub(MessagingContext context, ILogger<ChatHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var set = _connections.GetOrAdd(userId, _ => new System.Collections.Concurrent.ConcurrentDictionary<string, byte>());
                    set[Context.ConnectionId] = 0;
                    _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    if (_connections.TryGetValue(userId, out var set))
                    {
                        set.TryRemove(Context.ConnectionId, out _);
                        if (set.IsEmpty)
                        {
                            _connections.TryRemove(userId, out _);
                        }
                    }
                    _logger.LogInformation("User {UserId} disconnected", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
            return base.OnDisconnectedAsync(exception);
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

        // Birebir mesaj gönderme by receiver user id.
        public async Task SendPrivateMessageByUserId(string receiverUserId, string message)
        {
            var sender = Context.ConnectionId;
            if (!string.IsNullOrEmpty(receiverUserId) && TryGetConnectionIds(receiverUserId, out var connIds))
            {
                foreach (var receiverConnectionId in connIds)
                {
                    await Clients.Client(receiverConnectionId).SendAsync("ReceivePrivateMessage", sender, message);
                    await LogMessageAsync("Info", $"Private message from {sender} to {receiverConnectionId}: {message}");
                }
            }
            else
            {
                _logger.LogWarning("Receiver user {ReceiverUserId} not connected", receiverUserId);
            }
        }

        // Grup mesajlaşması: Belirli bir gruba mesaj gönderir.
        public async Task SendMessageToGroup(string groupName, string message)
        {
            var sender = Context.ConnectionId;
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", sender, message);
            await LogMessageAsync("Info", $"Group message in '{groupName}' from {sender}: {message}");
        }

        // Helper to get connection ids by user id
        public static bool TryGetConnectionIds(string userId, out IEnumerable<string> connectionIds)
        {
            connectionIds = Enumerable.Empty<string>();
            if (string.IsNullOrEmpty(userId)) return false;
            if (_connections.TryGetValue(userId, out var set))
            {
                connectionIds = set.Keys.ToList();
                return true;
            }
            return false;
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