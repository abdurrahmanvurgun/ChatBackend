using System;

namespace ChatApp.Backend.Models
{
    public class MessageLog
    {
        public guid Id { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}