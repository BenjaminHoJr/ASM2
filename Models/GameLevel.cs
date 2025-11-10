using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class GameLevel
    {
        [Key]
        public int LevelId { get; set; }
        public string title { get; set; }
        public string? Description { get; set; }
    }
}