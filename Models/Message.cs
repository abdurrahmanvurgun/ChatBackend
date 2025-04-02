using System;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Backend.Models
{
    public class Message
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Gönderen bilgisi gereklidir.")]
        public required string Sender { get; set; }

        [Required(ErrorMessage = "Alıcı bilgisi gereklidir.")]
        public required string Receiver { get; set; }

        [Required(ErrorMessage = "Mesaj içeriği gereklidir.")]
        public required string Content { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}