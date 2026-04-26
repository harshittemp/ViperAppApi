namespace ViperAppApi.Models.DTOs;

public class CommentDto
{
    public long CommentID { get; set; }
    public long PostID { get; set; }
    public long UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}