using System;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Backend.Models.Dto
{
    public class GroupInviteDto
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public Guid TargetUserId { get; set; }
    }
}