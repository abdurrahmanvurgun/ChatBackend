using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using ChatApp.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChatBackend.Data;
using Microsoft.Extensions.Logging;

namespace ChatApp.Backend.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly MessagingContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserService(IConfiguration configuration, MessagingContext context, ILogger<UserService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        // Private method ile log kaydı oluşturuluyor.
        private async Task LogDatabaseAsync(string level, string message, Guid? userId = null)
        {
            try
            {
                var log = new Log
                {
                    Level = level,
                    Message = message,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };
                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veritabanına log kaydı eklenirken hata oluştu.");
            }
        }

        public async Task<User> Register(UserRegistrationDto registrationDto)
        {
            _logger.LogInformation("Register request received for email: {Email}", registrationDto.Email);
            await LogDatabaseAsync("Info", $"Register request received for email: {registrationDto.Email}");

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == registrationDto.Email.ToLower());
            if (existingUser != null)
            {
                _logger.LogError("Registration failed: user with email {Email} already exists.", registrationDto.Email);
                await LogDatabaseAsync("Error", $"Registration failed: user with email {registrationDto.Email} already exists.");
                throw new InvalidOperationException("Bu email adresiyle kayıtlı bir kullanıcı zaten mevcut.");
            }

            var user = new User
            {
                Name = registrationDto.Name,
                Surname = registrationDto.Surname,
                Email = registrationDto.Email,
                Username = registrationDto.Username,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, registrationDto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully. UserId: {UserId}", user.Id);
            await LogDatabaseAsync("Info", $"User registered successfully. UserId: {user.Id}", user.Id);

            return user;
        }

        public async Task<string> Login(UserLoginDto loginDto)
        {
            _logger.LogInformation("Login attempt received for email: {Email}", loginDto.Email);
            await LogDatabaseAsync("Info", $"Login attempt received for email: {loginDto.Email}");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());
            if (user == null)
            {
                _logger.LogError("Login failed: user with email {Email} not found.", loginDto.Email);
                await LogDatabaseAsync("Error", $"Login failed: user with email {loginDto.Email} not found.");
                throw new InvalidOperationException("Kullanıcı bulunamadı.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (verificationResult != PasswordVerificationResult.Success)
            {
                _logger.LogError("Login failed for email {Email}: invalid credentials.", loginDto.Email);
                await LogDatabaseAsync("Error", $"Login failed for email {loginDto.Email}: invalid credentials.", user.Id);
                throw new InvalidOperationException("Giriş bilgileri geçersiz.");
            }

            var jwtKey = _configuration.GetValue<string>("Jwt:Key") ?? "VerySecretKey12345";
            var key = Encoding.ASCII.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("User login successful. UserId: {UserId}", user.Id);
            await LogDatabaseAsync("Info", $"User login successful. UserId: {user.Id}", user.Id);

            return tokenString;
        }
    }
}