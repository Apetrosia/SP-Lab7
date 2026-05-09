using GreenSwampApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSwampApp.Models
{
    [Table("events")]
    public class Event
    {
        [Key]
        [Column("event_id")]
        public long EventId { get; set; }

        [Column("post_id")]
        public long PostId { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("title")]
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("location")]
        [StringLength(300)]
        public string Location { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}