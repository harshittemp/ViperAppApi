using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViperAppApi.Models.Domains;

public class PostLike
{
    [Key]
    public long LikeID { get; set; }

    public long PostID { get; set; }
    public long UserID { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("PostID")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("UserID")]
    public virtual User User { get; set; } = null!;
}