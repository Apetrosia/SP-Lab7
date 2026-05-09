using GreenSwampApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSwampApp.Models
{
    [Table("tags")]
    public class Tag
    {
        [Key]
        [Column("tag_id")]
        public long TagId { get; set; }

        [Column("name")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<PostTag> PostTags { get; set; }
    }
}