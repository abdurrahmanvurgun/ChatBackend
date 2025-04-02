using System.ComponentModel.DataAnnotations;

namespace ChatApp.Backend.Models
{
    public class UserRegistrationDto
    {
        [Required(ErrorMessage = "İsim gereklidir.")]
        public required string Name { get; set; }
    
        [Required(ErrorMessage = "Soyisim gereklidir.")]
        public required string Surname { get; set; }
    
        [Required(ErrorMessage = "Email gerekli.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girin.")]
        public required string Email { get; set; }
    
        [Required(ErrorMessage = "Parola gereklidir.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Parola tekrarı gereklidir.")]
        [Compare("Password", ErrorMessage = "Parolalar uyuşmuyor.")]
        public required string ConfirmPassword { get; set; }
    
        
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olmalı.")]
        public string? Username { get; set; }
    }
}