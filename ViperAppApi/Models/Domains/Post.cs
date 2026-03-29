using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViperAppApi.Models.Domains;

public class Post
{
    [Key]
    public long PostID { get; set; }

    public long UserID { get; set; }

    public string? Content { get; set; }

    public string? ImageUrl { get; set; }

    public string? VideoUrl { get; set; }

    public string PostType { get; set; } // "image", "video", "text"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? CreatedBy { get; set; }

    // Navigation
    [ForeignKey("UserID")]
    public User User { get; set; }
}