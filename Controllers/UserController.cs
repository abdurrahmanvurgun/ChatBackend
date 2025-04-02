using Microsoft.AspNetCore.Mvc;
using ChatApp.Backend.Services;
using ChatApp.Backend.Models;

namespace ChatBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
    
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
    
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] UserRegistrationDto registrationDto)
        {
            if (registrationDto == null)
            {
                return BadRequest("Kayıt verisi boş olamaz.");
            }
    
            var user = await _userService.Register(registrationDto);
            if (user == null)
            {
                return BadRequest("Kayıt işlemi başarısız.");
            }
    
            return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
        }
    
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest("Giriş verisi boş olamaz.");
            }
    
            var token = await _userService.Login(loginDto);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Email veya parola hatalı.");
            }
    
            return Ok(new { Token = token });
        }
    }
}