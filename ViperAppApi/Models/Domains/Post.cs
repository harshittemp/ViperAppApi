using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViperAppApi.Models.Domains;

public class Post
{
    [Key]
    public long PostID { get; set; }

    public long UserID { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? MediaUrl { get; set; }  // URL for image or video

    public string? MediaType { get; set; }  // "image", "video", or null for text-only

    public int LikesCount { get; set; } = 0;

    public int CommentsCount { get; set; } = 0;

    public int SharesCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UserID")]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    //public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}