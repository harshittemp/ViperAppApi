using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViperAppApi.Models.Domains
{
    public class Notification
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; } // Who receives the notification

        public string Type { get; set; } = string.Empty; // like, comment, follow, mention, share

        public long SourceUserId { get; set; } // Who triggered the notification

        public string UserName { get; set; } = string.Empty;

        public string UserAvatar { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public long? TargetId { get; set; } // PostId, CommentId, etc.

        public string? TargetType { get; set; } // post, comment, story

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("SourceUserId")]
        public virtual User SourceUser { get; set; } = null!;
    }
}