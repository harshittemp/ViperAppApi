using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViperAppApi.Data;
using ViperAppApi.Models.Domains;
using ViperAppApi.Models.DTOs;

namespace ViperAppApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ✅ GET PROFILE - Returns user with image URL directly
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var userId = long.Parse(userIdClaim.Value);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Generate the image URL directly
            var profileImageUrl = GetProfileImageUrl(user);

            // Return user with image URL
            var response = new
            {
                user.UserID,
                user.UserName,
                user.Email,
                user.Bio,
                user.Website,
                user.Linkedin,
                user.Twitter,
                user.CreatedAt,
                hasProfileImage = user.ProfileImageData != null && user.ProfileImageData.Length > 0,
                profileImage = profileImageUrl  // Direct URL to the image
            };

            return Ok(response);
        }

        // ✅ UPDATE PROFILE
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "User ID not found in token" });

                var userId = long.Parse(userIdClaim);
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                // Update text fields
                if (!string.IsNullOrEmpty(dto.UserName))
                    user.UserName = dto.UserName;

                if (dto.Bio != null)
                    user.Bio = dto.Bio;

                if (dto.Website != null)
                    user.Website = dto.Website;

                if (dto.Linkedin != null)
                    user.Linkedin = dto.Linkedin;

                if (dto.Twitter != null)
                    user.Twitter = dto.Twitter;

                // Handle image upload
                if (dto.Image != null && dto.Image.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await dto.Image.CopyToAsync(memoryStream);
                        user.ProfileImageData = memoryStream.ToArray();
                        user.ProfileImageContentType = dto.Image.ContentType;
                    }
                    user.ProfileImage = "db";
                }

                await _context.SaveChangesAsync();

                // Generate the image URL
                var profileImageUrl = GetProfileImageUrl(user);

                // Return updated user with image URL
                return Ok(new
                {
                    user.UserID,
                    user.UserName,
                    user.Email,
                    user.Bio,
                    user.Website,
                    user.Linkedin,
                    user.Twitter,
                    user.CreatedAt,
                    hasProfileImage = user.ProfileImageData != null && user.ProfileImageData.Length > 0,
                    profileImage = profileImageUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error updating profile: {ex.Message}" });
            }
        }

        // ✅ GET USER IMAGE FROM DATABASE
        [AllowAnonymous]
        [HttpGet("image/{userId}")]
        public async Task<IActionResult> GetUserImage(long userId)
        {
            var user = await _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => new { u.ProfileImageData, u.ProfileImageContentType })
                .FirstOrDefaultAsync();

            if (user?.ProfileImageData == null || user.ProfileImageData.Length == 0)
            {
                return NotFound(new { message = "No profile image found" });
            }

            var contentType = user.ProfileImageContentType ?? "image/png";
            return File(user.ProfileImageData, contentType);
        }

        // ✅ GET USER BY ID (for displaying other users)
        [AllowAnonymous]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(long userId)
        {
            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            long? currentUserId = currentUserIdClaim != null ? long.Parse(currentUserIdClaim) : null;

            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            bool isFollowing = false;
            bool isFriend = false;

            if (currentUserId.HasValue)
            {
                isFollowing = await _context.Set<Follow>()
                    .AnyAsync(f => f.FollowerId == currentUserId.Value && f.FollowingId == userId);

                isFriend = await _context.FriendRequests
                    .AnyAsync(fr => (fr.SenderId == currentUserId.Value && fr.ReceiverId == userId ||
                                     fr.SenderId == userId && fr.ReceiverId == currentUserId.Value)
                                     && fr.Status == "accepted");
            }

            var profileImageUrl = GetProfileImageUrl(user);

            var userDto = new
            {
                user.UserID,
                user.UserName,
                user.Bio,
                profileImage = profileImageUrl,
                hasProfileImage = user.ProfileImageData != null && user.ProfileImageData.Length > 0,
                isFollowing = isFollowing,
                isFriend = isFriend,
                followersCount = user.Followers?.Count ?? 0,
                followingCount = user.Following?.Count ?? 0
            };

            return Ok(userDto);
        }

        // Helper method to get profile image URL
        private string? GetProfileImageUrl(User user)
        {
            if (user.ProfileImageData != null && user.ProfileImageData.Length > 0)  
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                return $"{baseUrl}/api/user/image/{user.UserID}";
            }
            return null;
        }
    }
}