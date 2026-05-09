using GreenSwampApp.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSwampApp.Models
{
    [Table("followers")]
    public class Follower
    {
        [Column("follower_id")]
        public long FollowerId { get; set; }

        [Column("following_id")]
        public long FollowingId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("FollowerId")]
        public virtual User FollowerUser { get; set; }

        [ForeignKey("FollowingId")]
        public virtual User FollowingUser { get; set; }
    }
}