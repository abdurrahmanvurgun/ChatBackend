using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChatBackend.Data;
using ChatApp.Backend.Models;
using ChatApp.Backend.Models.Dto;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using ChatApp.Backend.Hubs;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly MessagingContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public GroupsController(MessagingContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Create a new group. The authenticated user becomes the Owner.
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateGroup([FromBody] GroupDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var ownerId = Guid.Parse(userIdClaim);

            var group = new Group { Name = dto.Name, OwnerId = ownerId };
            _context.Groups.Add(group);
            // Owner is automatically an approved member
            _context.GroupMembers.Add(new GroupMember { GroupId = group.Id, UserId = ownerId, Status = ChatApp.Backend.Models.MembershipStatus.Approved });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetGroup(Guid id)
        {
            var group = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();
            return Ok(group);
        }

        // Invite a user to a group (only owner can invite)
        [HttpPost("invite")]
        [Authorize]
        public async Task<IActionResult> Invite([FromBody] GroupInviteDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var requesterId = Guid.Parse(userIdClaim);

            var group = await _context.Groups.FindAsync(dto.GroupId);
            if (group == null) return NotFound("Group not found");
            // allow if owner or global admin
            var requester = await _context.Users.FindAsync(requesterId);
            if (group.OwnerId != requesterId && (requester == null || !requester.IsAdmin)) return Forbid();

            // check target exists
            var target = await _context.Users.FindAsync(dto.TargetUserId);
            if (target == null) return NotFound("Target user not found");

            // create invite (unapproved membership)
            var existing = await _context.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == dto.GroupId && m.UserId == dto.TargetUserId);
            if (existing != null) return Conflict("User already invited or is a member");

            var invite = new GroupMember { GroupId = dto.GroupId, UserId = dto.TargetUserId, Status = ChatApp.Backend.Models.MembershipStatus.Pending };
            _context.GroupMembers.Add(invite);
            await _context.SaveChangesAsync();

            // notify target via SignalR if connected (by user id)
            if (ChatHub.TryGetConnectionIds(dto.TargetUserId.ToString(), out var connIds))
            {
                foreach (var connId in connIds)
                {
                    await _hubContext.Clients.Client(connId).SendAsync("GroupInviteReceived", dto.GroupId, group.Name);
                }
            }

            return Ok(invite);
        }

        // Target user responds to the invite (accept)
        [HttpPost("respond/{groupId}")]
        [Authorize]
        public async Task<IActionResult> RespondInvite(Guid groupId, [FromQuery] bool accept = true)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var userId = Guid.Parse(userIdClaim);

            var membership = await _context.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
            if (membership == null) return NotFound("Invite not found");
            if (membership.Status == ChatApp.Backend.Models.MembershipStatus.Approved) return BadRequest("Already a member");

            membership.Status = accept ? ChatApp.Backend.Models.MembershipStatus.Approved : ChatApp.Backend.Models.MembershipStatus.Rejected;
            membership.RespondedAt = DateTime.UtcNow;
            membership.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notify group owner that membership was responded to
            var group = await _context.Groups.FindAsync(groupId);
            if (group != null)
            {
                if (ChatHub.TryGetConnectionIds(group.OwnerId.ToString(), out var connIds))
                {
                    foreach (var c in connIds)
                    {
                        await _hubContext.Clients.Client(c).SendAsync("GroupMemberResponded", groupId, userId, membership.Status.ToString());
                    }
                }
            }

            return Ok(membership);
        }

        // Invitee rejects the invite
        [HttpPost("decline/{groupId}")]
        [Authorize]
        public async Task<IActionResult> DeclineInvite(Guid groupId)
        {
            // Simply call respond with accept=false for clarity
            return await RespondInvite(groupId, accept: false);
        }

        // Owner cancels an invite (before accepted)
        [HttpPost("cancel/{groupId}")]
        [Authorize]
        public async Task<IActionResult> CancelInvite(Guid groupId, [FromBody] Guid targetUserId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var requesterId = Guid.Parse(userIdClaim);

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null) return NotFound("Group not found");
            if (group.OwnerId != requesterId) return Forbid();

            var membership = await _context.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == targetUserId);
            if (membership == null) return NotFound("Invite not found");

            // If already approved, we won't delete membership here (could implement leave/remove separately)
            if (membership.Status == ChatApp.Backend.Models.MembershipStatus.Approved) return BadRequest("User already a member");

            membership.Status = ChatApp.Backend.Models.MembershipStatus.Cancelled;
            membership.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notify target user
            if (ChatHub.TryGetConnectionIds(targetUserId.ToString(), out var connIds))
            {
                foreach (var c in connIds)
                {
                    await _hubContext.Clients.Client(c).SendAsync("GroupInviteCancelled", groupId);
                }
            }

            return Ok(membership);
        }

        // List invite history for a user (either invites sent by owner or received by user)
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> InviteHistory([FromQuery] bool sentByMe = false)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var userId = Guid.Parse(userIdClaim);

            if (sentByMe)
            {
                // invites that this user has sent as owner
                var groups = await _context.Groups.Where(g => g.OwnerId == userId).SelectMany(g => _context.GroupMembers.Where(m => m.GroupId == g.Id)).ToListAsync();
                return Ok(groups);
            }
            else
            {
                // invites received by the user
                var invites = await _context.GroupMembers.Where(m => m.UserId == userId).ToListAsync();
                return Ok(invites);
            }
        }
    }
}
