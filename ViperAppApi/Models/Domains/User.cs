using System.ComponentModel.DataAnnotations;

namespace ViperAppApi.Models.Domains;

public class User
{
    [Key]
    public long UserID { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string Email { get; set; }

    // ✅ FIXED (hashed password)
    public string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? CreatedBy { get; set; }

    // Navigation
    public ICollection<Post> Posts { get; set; }
}