using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChatBackend.Data;
using ChatApp.Backend.Models;

namespace ChatBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly MessagingContext _context;

        public AdminController(MessagingContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public ActionResult<IEnumerable<object>> GetAllUsers()
        {
            var users = _context.Users
                .Select(u => new {
                    u.Id,
                    u.Name,
                    u.Surname,
                    u.Email,
                    u.Username,
                    u.DisplayName,
                    u.ProfilePictureUrl,
                    u.CreatedAt,
                    u.IsAdmin
                })
                .ToList();

            return Ok(users);
        }

        [HttpGet("messages")]
        public ActionResult<IEnumerable<Message>> GetAllMessages()
        {
            var messages = _context.Messages.OrderByDescending(m => m.Timestamp).ToList();
            return Ok(messages);
        }
    }
}
