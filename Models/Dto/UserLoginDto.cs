using System.ComponentModel.DataAnnotations;

namespace ChatApp.Backend.Models
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Email gerekli.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girin.")]
        public required string Email { get; set; }
    
        [Required(ErrorMessage = "Parola gereklidir.")]
        public required string Password { get; set; }
    }
}