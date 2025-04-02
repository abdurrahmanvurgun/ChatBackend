using System;

namespace ChatApp.Backend.Models.Dtos
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid Sender { get; set; }
        public Guid Receiver { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}