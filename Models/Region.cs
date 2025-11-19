using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Region
    {
        [Key]
        [Column("RegionId")]
        public int regionId { get; set; }

        [Required]
        public required string regionName { get; set; }
    }
}