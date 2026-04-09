using Microsoft.EntityFrameworkCore;
using ViperAppApi.Data;
using ViperAppApi.Interfaces;
using ViperAppApi.Models.Domains;
using ViperAppApi.Models.DTOs;

namespace ViperAppApi.Services
{
    public class SocialService : ISocialService
    {
        private readonly AppDbContext _context;

        public SocialService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync(int currentUserId, string search = "")
        {
            var query = _context.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UserName.Contains(search) ||
                                         u.Email.Contains(search) ||
                                         (u.Bio != null && u.Bio.Contains(search)));
            }

            var users = await query
                .Select(u => new UserDto
                {
                    UserID = u.UserID,
                    UserName = u.UserName,
                    ProfileImage = u.ProfileImage,
                    Bio = u.Bio,
                    FollowersCount = _context.Follows.Count(f => f.FollowingId == u.UserID),
                    FollowingCount = _context.Follows.Count(f => f.FollowerId == u.UserID),
                    // ✅ Fixed: Use direct expressions instead of method calls
                    IsFollowing = _context.Follows.Any(f => f.FollowerId == currentUserId && f.FollowingId == u.UserID),
                    IsFriend = _context.Friends.Any(f =>
                        (f.UserId == currentUserId && f.FriendId == u.UserID) ||
                        (f.UserId == u.UserID && f.FriendId == currentUserId))
                })
                .ToListAsync();

            return users;
        }

        public async Task<IEnumerable<UserDto>> GetRecommendationsAsync(int currentUserId)
        {
            var recommendations = await _context.Users
                .Where(u => u.UserID != currentUserId)
                .Where(u => !_context.Follows.Any(f => f.FollowerId == currentUserId && f.FollowingId == u.UserID))
                .Where(u => !_context.Friends.Any(f =>
                    (f.UserId == currentUserId && f.FriendId == u.UserID) ||
                    (f.UserId == u.UserID && f.FriendId == currentUserId)))
                .OrderByDescending(u => _context.Follows.Count(f => f.FollowingId == u.UserID))
                .Take(10)
                .Select(u => new UserDto
                {
                    UserID = u.UserID,
                    UserName = u.UserName,
                    ProfileImage = u.ProfileImage,
                    Bio = u.Bio,
                    FollowersCount = _context.Follows.Count(f => f.FollowingId == u.UserID),
                    FollowingCount = _context.Follows.Count(f => f.FollowerId == u.UserID),
                    IsFollowing = false,
                    IsFriend = false
                })
                .ToListAsync();

            return recommendations;
        }

        public async Task<FollowResponseDto> FollowUserAsync(int currentUserId, int userIdToFollow)
        {
            // Cannot follow yourself
            if (currentUserId == userIdToFollow)
            {
                return new FollowResponseDto
                {
                    Success = false,
                    Message = "Cannot follow yourself",
                    FollowersCount = 0
                };
            }

            // Check if already following
            if (IsFollowing(currentUserId, userIdToFollow))
            {
                return new FollowResponseDto
                {
                    Success = false,
                    Message = "Already following this user",
                    FollowersCount = 0
                };
            }

            var follow = new Follow
            {
                FollowerId = currentUserId,
                FollowingId = userIdToFollow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            var followersCount = await _context.Follows.CountAsync(f => f.FollowingId == userIdToFollow);

            return new FollowResponseDto
            {
                Success = true,
                Message = "Successfully followed user",
                FollowersCount = followersCount
            };
        }

        public async Task<FollowResponseDto> UnfollowUserAsync(int currentUserId, int userIdToUnfollow)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == userIdToUnfollow);

            if (follow == null)
            {
                return new FollowResponseDto
                {
                    Success = false,
                    Message = "Not following this user",
                    FollowersCount = 0
                };
            }

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            var followersCount = await _context.Follows.CountAsync(f => f.FollowingId == userIdToUnfollow);

            return new FollowResponseDto
            {
                Success = true,
                Message = "Successfully unfollowed user",
                FollowersCount = followersCount
            };
        }

        public async Task<FriendRequestDto> SendFriendRequestAsync(int currentUserId, int recipientId)
        {
            // Cannot send request to yourself
            if (currentUserId == recipientId)
            {
                return null;
            }

            // Check if request already exists
            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(r =>
                    (r.SenderId == currentUserId && r.ReceiverId == recipientId) ||
                    (r.SenderId == recipientId && r.ReceiverId == currentUserId));

            if (existingRequest != null)
            {
                return null;
            }

            var request = new FriendRequest
            {
                SenderId = currentUserId,
                ReceiverId = recipientId,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(request);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync((long)currentUserId);

            return new FriendRequestDto
            {
                RequestId = (int)request.RequestId,
                SenderId = (int)request.SenderId,
                SenderName = sender?.UserName ?? string.Empty,
                SenderImage = sender?.ProfileImage ?? string.Empty,
                Status = request.Status,
                CreatedAt = request.CreatedAt
            };
        }

        public async Task<bool> AcceptFriendRequestAsync(int currentUserId, int requestId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.ReceiverId == currentUserId && r.Status == "pending");

            if (request == null)
                return false;

            request.Status = "accepted";
            request.RespondedAt = DateTime.UtcNow;

            // Add to friends table
            var friendship = new Friend
            {
                UserId = (int)request.SenderId,
                FriendId = (int)request.ReceiverId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friends.Add(friendship);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectFriendRequestAsync(int currentUserId, int requestId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.ReceiverId == currentUserId && r.Status == "pending");

            if (request == null)
                return false;

            request.Status = "rejected";
            request.RespondedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(int currentUserId)
        {
            var requests = await _context.FriendRequests
                .Where(r => r.ReceiverId == currentUserId && r.Status == "pending")
                .Include(r => r.Sender)
                .Select(r => new FriendRequestDto
                {
                    RequestId = (int)r.RequestId,
                    SenderId = (int)r.SenderId,
                    SenderName = r.Sender.UserName ?? string.Empty,
                    SenderImage = r.Sender.ProfileImage ?? string.Empty,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return requests;
        }

        public async Task<IEnumerable<UserDto>> GetFollowersAsync(int currentUserId)
        {
            var followers = await _context.Follows
                .Where(f => f.FollowingId == currentUserId)
                .Include(f => f.Follower)
                .Select(f => new UserDto
                {
                    UserID = f.Follower.UserID,
                    UserName = f.Follower.UserName,
                    ProfileImage = f.Follower.ProfileImage,
                    Bio = f.Follower.Bio,
                    // ✅ Fixed: Use direct expressions
                    IsFollowing = _context.Follows.Any(fol => fol.FollowerId == currentUserId && fol.FollowingId == f.Follower.UserID),
                    IsFriend = _context.Friends.Any(fr =>
                        (fr.UserId == currentUserId && fr.FriendId == f.Follower.UserID) ||
                        (fr.UserId == f.Follower.UserID && fr.FriendId == currentUserId))
                })
                .ToListAsync();

            return followers;
        }

        public async Task<IEnumerable<UserDto>> GetFollowingAsync(int currentUserId)
        {
            var following = await _context.Follows
                .Where(f => f.FollowerId == currentUserId)
                .Include(f => f.Following)
                .Select(f => new UserDto
                {
                    UserID = f.Following.UserID,
                    UserName = f.Following.UserName,
                    ProfileImage = f.Following.ProfileImage,
                    Bio = f.Following.Bio,
                    IsFollowing = true,
                    // ✅ Fixed: Use direct expressions
                    IsFriend = _context.Friends.Any(fr =>
                        (fr.UserId == currentUserId && fr.FriendId == f.Following.UserID) ||
                        (fr.UserId == f.Following.UserID && fr.FriendId == currentUserId))
                })
                .ToListAsync();

            return following;
        }

        public async Task<UserStatsDto> GetUserStatsAsync(int currentUserId)
        {
            var stats = new UserStatsDto
            {
                FollowersCount = await _context.Follows.CountAsync(f => f.FollowingId == currentUserId),
                FollowingCount = await _context.Follows.CountAsync(f => f.FollowerId == currentUserId),
                PostsCount = await _context.Posts.CountAsync(p => p.UserID == currentUserId),
                FriendsCount = await _context.Friends.CountAsync(f => f.UserId == currentUserId || f.FriendId == currentUserId)
            };

            return stats;
        }

        public bool IsFollowing(int currentUserId, int userId)
        {
            return _context.Follows.Any(f => f.FollowerId == currentUserId && f.FollowingId == userId);
        }

        public bool IsFriend(int currentUserId, int userId)
        {
            return _context.Friends.Any(f =>
                (f.UserId == currentUserId && f.FriendId == userId) ||
                (f.UserId == userId && f.FriendId == currentUserId));
        }
    }
}