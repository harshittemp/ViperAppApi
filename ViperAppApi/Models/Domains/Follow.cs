using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViperAppApi.Models.Domains;

public class Follow
{
    [Key]
    public long FollowId { get; set; }

    public long FollowerId { get; set; } // User who follows
    public long FollowingId { get; set; } // User being followed

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("FollowerId")]
    public virtual User Follower { get; set; } = null!;

    [ForeignKey("FollowingId")]
    public virtual User Following { get; set; } = null!;
}
