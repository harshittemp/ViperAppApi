namespace ViperAppApi.Models.DTOs;

public class FriendRequestDto
{
    public long RequestId { get; set; }
    public long SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderImage { get; set; }
    public long ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
