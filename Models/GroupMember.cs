using System;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Backend.Models
{
    public enum MembershipStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3
    }

    public class GroupMember
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        // Membership status: Pending / Approved / Rejected / Cancelled
        public MembershipStatus Status { get; set; } = MembershipStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}