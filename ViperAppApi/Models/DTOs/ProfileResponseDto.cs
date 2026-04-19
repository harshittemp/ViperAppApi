namespace ViperAppApi.Models.DTOs;

public class ProfileResponseDto
{
    public long UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? Linkedin { get; set; }
    public string? Twitter { get; set; }
    public DateTime CreatedAt { get; set; }
}
