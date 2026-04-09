using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViperAppApi.Models.Domains;

public class CommentLike
{
    [Key]
    public long CommentLikeID { get; set; }

    public long CommentID { get; set; }
    public long UserID { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CommentID")]
    public virtual Comment Comment { get; set; } = null!;

    [ForeignKey("UserID")]
    public virtual User User { get; set; } = null!;
}