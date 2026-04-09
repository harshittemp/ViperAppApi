namespace ViperAppApi.Models.DTOs;

public class PostDto
{
    public long PostID { get; set; }
    public long UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserImage { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public int SharesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsLiked { get; set; }
}