namespace ViperAppApi.Models.DTOs;

public class UserDto
{
    public long UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool HasProfileImage { get; set; }
    public string? Bio { get; set; }
    public bool IsFollowing { get; set; }
    public bool IsFriend { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
}


// Follow Response DTO
public class FollowResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int FollowersCount { get; set; }
}

// User Stats DTO
public class UserStatsDto
{
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public int PostsCount { get; set; }
    public int FriendsCount { get; set; }
}
