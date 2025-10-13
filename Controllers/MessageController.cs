using Microsoft.AspNetCore.Mvc;
using ChatBackend.Services;
using ChatApp.Backend.Models.Dtos;
using Microsoft.AspNetCore.SignalR;
using ChatApp.Backend.Hubs;

namespace ChatBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(IMessageService messageService, IHubContext<ChatHub> hubContext)
        {
            _messageService = messageService;
            _hubContext = hubContext;
        }

        [HttpGet("receiver/{receiverId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForReceiver(string receiverId)
        {
            var messages = await _messageService.GetMessages(receiverId);
            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<ActionResult<MessageDto>> SendMessage([FromBody] MessageDto dto)
        {
            // Verify authenticated sender matches DTO sender
            var authUserId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (authUserId == null || authUserId != dto.Sender.ToString())
            {
                return Forbid();
            }

            var saved = await _messageService.SendMessage(dto);

            // Notify via SignalR to all recipient connections if any
            var receiverUserId = saved.Receiver.ToString();
            if (ChatApp.Backend.Hubs.ChatHub.TryGetConnectionIds(receiverUserId, out var connIds))
            {
                foreach (var id in connIds)
                {
                    await _hubContext.Clients.Client(id).SendAsync("ReceivePrivateMessage", saved.Sender, saved.Content);
                }
            }

            return CreatedAtAction(nameof(SendMessage), new { id = saved.Id }, saved);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(Guid id)
        {
            var ok = await _messageService.DeleteMessage(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
