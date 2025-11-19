using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class GameLevel
    {
        [Key]
        [Column("LevelId")]
        public int levelId { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();

        // Thêm property navigation cho GameResults (được dùng trong ApplicationDbContext)
        public ICollection<GameResult> GameResults { get; set; } = new List<GameResult>();
    }
}