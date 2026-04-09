using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ViperAppApi.Interfaces;
using ViperAppApi.Models.DTOs;

namespace ViperAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SocialController : ControllerBase
    {
        private readonly ISocialService _socialService;

        public SocialController(ISocialService socialService)
        {
            _socialService = socialService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] string search = "")
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var users = await _socialService.GetUsersAsync(currentUserId, search);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving users", error = ex.Message });
            }
        }

        [HttpGet("recommendations")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetRecommendations()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var recommendations = await _socialService.GetRecommendationsAsync(currentUserId);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving recommendations", error = ex.Message });
            }
        }

        [HttpPost("follow/{userId}")]
        public async Task<ActionResult<FollowResponseDto>> FollowUser(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _socialService.FollowUserAsync(currentUserId, userId);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error following user", error = ex.Message });
            }
        }

        [HttpDelete("unfollow/{userId}")]
        public async Task<ActionResult<FollowResponseDto>> UnfollowUser(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _socialService.UnfollowUserAsync(currentUserId, userId);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error unfollowing user", error = ex.Message });
            }
        }

        [HttpPost("friend-request/{userId}")]
        public async Task<ActionResult<FriendRequestDto>> SendFriendRequest(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var request = await _socialService.SendFriendRequestAsync(currentUserId, userId);

                if (request == null)
                    return BadRequest(new { message = "Unable to send friend request" });

                return Ok(request);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending friend request", error = ex.Message });
            }
        }

        [HttpPut("friend-request/{requestId}/accept")]
        public async Task<ActionResult> AcceptFriendRequest(int requestId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _socialService.AcceptFriendRequestAsync(currentUserId, requestId);

                if (!result)
                    return BadRequest(new { message = "Unable to accept friend request" });

                return Ok(new { message = "Friend request accepted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error accepting friend request", error = ex.Message });
            }
        }

        [HttpDelete("friend-request/{requestId}/reject")]
        public async Task<ActionResult> RejectFriendRequest(int requestId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _socialService.RejectFriendRequestAsync(currentUserId, requestId);

                if (!result)
                    return BadRequest(new { message = "Unable to reject friend request" });

                return Ok(new { message = "Friend request rejected" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error rejecting friend request", error = ex.Message });
            }
        }

        [HttpGet("friend-requests/pending")]
        public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetPendingRequests()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var requests = await _socialService.GetPendingRequestsAsync(currentUserId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving pending requests", error = ex.Message });
            }
        }

        [HttpGet("followers")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetFollowers()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var followers = await _socialService.GetFollowersAsync(currentUserId);
                return Ok(followers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving followers", error = ex.Message });
            }
        }

        [HttpGet("following")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetFollowing()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var following = await _socialService.GetFollowingAsync(currentUserId);
                return Ok(following);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving following", error = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<UserStatsDto>> GetUserStats()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var stats = await _socialService.GetUserStatsAsync(currentUserId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user stats", error = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token");

            return int.Parse(userIdClaim);
        }
    }
}