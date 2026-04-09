using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViperAppApi.Models.Domains;

public class FriendRequest
{
    [Key]
    public long RequestId { get; set; }

    public long SenderId { get; set; } // User who sends the request
    public long ReceiverId { get; set; } // User who receives the request

    public string Status { get; set; } = "pending"; // pending, accepted, rejected, blocked
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    // Navigation properties
    [ForeignKey("SenderId")]
    public virtual User Sender { get; set; } = null!;

    [ForeignKey("ReceiverId")]
    public virtual User Receiver { get; set; } = null!;
}
