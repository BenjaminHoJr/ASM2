using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Question
    {
        [Key]
        [Column("QuestionId")]
        public int questionId { get; set; }

        public string? ContentQuestion { get; set; }
        public string? Answer { get; set; }
        public string? Option1 { get; set; }
        public string? Option2 { get; set; }
        public string? Option3 { get; set; }
        public string? Option4 { get; set; }

        [Column("LevelId")]
        public int LevelId { get; set; }

        [ForeignKey("LevelId")]
        public GameLevel? Level { get; set; }
    }
}
