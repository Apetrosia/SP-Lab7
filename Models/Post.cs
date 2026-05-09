using GreenSwampApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSwampApp.Models
{
    [Table("posts")]
    public class Post
    {
        [Key]
        [Column("post_id")]
        public long PostId { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("content")]
        [Required]
        public string Content { get; set; }

        [Column("media_url")]
        [StringLength(500)]
        public string MediaUrl { get; set; }

        [Column("media_type")]
        [StringLength(20)]
        public string MediaType { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<Interaction> Interactions { get; set; }
        public virtual ICollection<PostTag> PostTags { get; set; }
        public virtual Event Event { get; set; }
    }
}