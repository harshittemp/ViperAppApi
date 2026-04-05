using System.ComponentModel.DataAnnotations;

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
    public string? ProfileImage { get; set; }

    // Navigation - FIXED TYPO
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}