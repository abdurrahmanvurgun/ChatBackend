using System;

namespace ChatApp.Backend.Models
{
    public class MessageLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}