namespace ViperAppApi.Models.DTOs;

public class UpdateProfileDto
{
    public string UserName { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? Linkedin { get; set; }
    public string? Twitter { get; set; }
    public IFormFile? Image { get; set; }
}
