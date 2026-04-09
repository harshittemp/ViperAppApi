using ViperAppApi.Models.DTOs;

namespace ViperAppApi.Interfaces
{
    public interface ISocialService
    {
        Task<IEnumerable<UserDto>> GetUsersAsync(int currentUserId, string search = "");
        Task<IEnumerable<UserDto>> GetRecommendationsAsync(int currentUserId);
        Task<FollowResponseDto> FollowUserAsync(int currentUserId, int userIdToFollow);
        Task<FollowResponseDto> UnfollowUserAsync(int currentUserId, int userIdToUnfollow);
        Task<FriendRequestDto> SendFriendRequestAsync(int currentUserId, int recipientId);
        Task<bool> AcceptFriendRequestAsync(int currentUserId, int requestId);
        Task<bool> RejectFriendRequestAsync(int currentUserId, int requestId);
        Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(int currentUserId);
        Task<IEnumerable<UserDto>> GetFollowersAsync(int currentUserId);
        Task<IEnumerable<UserDto>> GetFollowingAsync(int currentUserId);
        Task<UserStatsDto> GetUserStatsAsync(int currentUserId);
        bool IsFollowing(int currentUserId, int userId);
        bool IsFriend(int currentUserId, int userId);
    }
}
