using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Backend.Models
{
    public class Group
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Owner (creator) of the group
        [Required]
        public Guid OwnerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<GroupMember> Members { get; set; } = new();
    }
}