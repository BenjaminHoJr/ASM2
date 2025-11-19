using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class GameResult
    {
        [Key]
        [Column("QuizzResultId")]
        public int resultId { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("LevelId")]
        public int LevelId { get; set; }

        public int Score { get; set; }

        [Column("CompletionDate")]
        public DateTime CompletionDate { get; set; }

        // Compatibility alias for views expecting CompletedAt
        [NotMapped]
        public DateTime CompletedAt { get => CompletionDate; set => CompletionDate = value; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("LevelId")]
        public GameLevel? Level { get; set; }
    }
}