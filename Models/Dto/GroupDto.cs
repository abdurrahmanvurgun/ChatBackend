using System;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Backend.Models.Dto
{
    public class GroupDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}