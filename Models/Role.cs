using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class Role
    {
        [Key]
        [Column("RoleId")]
        public int roleId { get; set; }

        // mapped to DB but hidden from JSON responses
        [JsonIgnore]
        public string? Name { get; set; }

        // expose this property in JSON instead
        [NotMapped]
        [JsonPropertyName("roleName")]
        public string? RoleName { get => Name; set => Name = value; }

        public string? Description { get; set; }
    }
}