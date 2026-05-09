using GreenSwampApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSwampApp.Models
{
    [Table("interactions")]
    public class Interaction
    {
        [Key]
        [Column("interaction_id")]
        public long InteractionId { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("post_id")]
        public long PostId { get; set; }

        [Column("interaction_type")]
        [Required]
        [StringLength(20)]
        public string InteractionType { get; set; } // 'like', 'reribb', 'comment'

        [Column("comment_content")]
        public string CommentContent { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }
    }
}