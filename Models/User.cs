using System;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Backend.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "İsim gereklidir.")]
        [StringLength(50, ErrorMessage = "İsim alanı en fazla 50 karakter olmalı.")]
        public string Name { get; set; }= string.Empty;
        
        [Required(ErrorMessage = "Soyisim gereklidir.")]
        [StringLength(50, ErrorMessage = "Soyisim alanı en fazla 50 karakter olmalı.")]
        public string Surname { get; set; }= string.Empty;

        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olmalı.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Email gerekli.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girin.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Parola hash gereklidir.")]
        public string PasswordHash { get; set; }= string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Opsiyonel alanlar
        [StringLength(100, ErrorMessage = "Görünen isim en fazla 100 karakter olabilir.")]
        public string? DisplayName { get; set; }
        
        [Url(ErrorMessage = "Geçerli bir URL girin.")]
        public string? ProfilePictureUrl { get; set; }

        // Global admin flag
        public bool IsAdmin { get; set; } = false;
    }
}