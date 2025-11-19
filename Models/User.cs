using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class User
    {
        [Key]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; } = default!;
        [Required]
        public string? Email { get; set; }

        // dùng tên chuẩn để EF nhận đúng FK (pascal-case)
        [Column("RegionId")]
        public int? RegionId { get; set; }

        [Column("RoleId")]
        public int? RoleId { get; set; }

        public string? LinkAvatar { get; set; }
        public bool IsDeleted { get; set; }

        [Column("OTP")]
        public string? OTP { get; set; }

        [NotMapped]
        [JsonIgnore]
        public string username { get => UserName; set => UserName = value; }

        [NotMapped]
        [JsonIgnore]
        public string? linkAvatar { get => LinkAvatar; set => LinkAvatar = value; }

        [NotMapped]
        [JsonIgnore]
        public string? otp { get => OTP; set => OTP = value; }

        // navigation props - dùng PascalCase
        [ForeignKey(nameof(RegionId))]
        public Region? Region { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        public ICollection<GameResult> GameResults { get; set; } = new List<GameResult>();
    }
}