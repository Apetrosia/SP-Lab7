using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSwampApp.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public long UserId { get; set; }

        [Column("username")]
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Column("email")]
        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Column("password_hash")]
        [Required]
        public string PasswordHash { get; set; }

        [Column("display_name")]
        [StringLength(100)]
        public string DisplayName { get; set; }

        [Column("bio")]
        public string Bio { get; set; }

        [Column("avatar_url")]
        [StringLength(500)]
        public string AvatarUrl { get; set; }

        [Column("cover_image_url")]
        [StringLength(500)]
        public string CoverImageUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Event> Events { get; set; }
        public virtual ICollection<Interaction> Interactions { get; set; }

        // Users that follow THIS user
        public virtual ICollection<Follower> Followers { get; set; }

        // Users that THIS user follows
        public virtual ICollection<Follower> Following { get; set; }
    }
}