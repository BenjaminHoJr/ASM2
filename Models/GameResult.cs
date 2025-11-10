using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class GameResult
    {
        [Key]
        public int ResultId { get; set; }
        public int UserId { get; set; }
        public int LevelId { get; set; }
        public int Score { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}