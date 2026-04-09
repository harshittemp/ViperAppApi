using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViperAppApi.Models.Domains;

public class Comment
{
    [Key]
    public long CommentID { get; set; }

    public long PostID { get; set; }
    public long UserID { get; set; }
    public long? ParentCommentID { get; set; }

    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    public int LikesCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    [ForeignKey("PostID")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("UserID")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("ParentCommentID")]
    public virtual Comment ParentComment { get; set; } = null!;

    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public virtual ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
}