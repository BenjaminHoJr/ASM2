using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        [Required]
        public string RoleName { get; set; }
        public string? Description { get; set; }
    }
}