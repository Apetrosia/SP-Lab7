using GreenSwampApp.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSwampApp.Models
{
    [Table("post_tags")]
    public class PostTag
    {
        [Column("post_id")]
        public long PostId { get; set; }

        [Column("tag_id")]
        public long TagId { get; set; }

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }
    }
}