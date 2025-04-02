using System;

namespace ChatApp.Backend.Models
{
    public class Log
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Level { get; set; }=string.Empty; // Log seviyesi (Info, Warning, Error vb.)
        public  string Message { get; set; }=string.Empty; // Log mesajı
        public string? Source { get; set; } // Log kaynağı (örneğin, hangi sınıf veya metot)
        public string? IpAddress { get; set; } // IP adresi (varsa)
        public string? UserAgent { get; set; } // Tarayıcı bilgisi (varsa)
        public string? Exception { get; set; } // Hata detayları için opsiyonel alan
        public Guid? UserId { get; set; } // Log ile ilişkilendirilecek kullanıcı (varsa)
    }
}