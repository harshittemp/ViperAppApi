using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViperAppApi.Models.Domains;

public class User
{
    [Key]
    public long UserID { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    // ✅ FIXED (hashed password)
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? CreatedBy { get; set; }

    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? Linkedin { get; set; }
    public string? Twitter { get; set; }

    // Keep for backward compatibility, but now it will be null or "db"
    public string? ProfileImage { get; set; }

    // NEW: Store image as byte array in database
    public byte[]? ProfileImageData { get; set; }

    // NEW: Store content type (e.g., "image/jpeg")
    public string? ProfileImageContentType { get; set; }

    // Navigation 
    public ICollection<Post> Posts { get; set; } = new List<Post>();

    // Users that this user follows
    [InverseProperty("Follower")]
    public ICollection<Follow> Following { get; set; } = new List<Follow>();

    // Users that follow this user
    [InverseProperty("Following")]
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();

    // Friend requests sent by this user
    [InverseProperty("Sender")]
    public ICollection<FriendRequest> SentRequests { get; set; } = new List<FriendRequest>();

    // Friend requests received by this user
    [InverseProperty("Receiver")]
    public ICollection<FriendRequest> ReceivedRequests { get; set; } = new List<FriendRequest>();
}