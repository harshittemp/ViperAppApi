using Microsoft.AspNetCore.Mvc;
using ViperAppApi.Services;
using System.Security.Claims;

namespace ViperAppApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly PostService _postService;
    private readonly ILogger<PostsController> _logger;

    public PostsController(PostService postService, ILogger<PostsController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPosts()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var posts = await _postService.GetAllPosts(currentUserId);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all posts");
            return StatusCode(500, new { message = "Error retrieving posts" });
        }
    }

    [HttpGet("user-posts")]
    public async Task<IActionResult> GetUserPosts()
    {
        try
        {
            var userId = GetCurrentUserId();
            var posts = await _postService.GetUserPosts(userId);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user posts");
            return StatusCode(500, new { message = "Error retrieving user posts" });
        }
    }

    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingPosts()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var posts = await _postService.GetTrendingPosts(currentUserId);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending posts");
            return StatusCode(500, new { message = "Error retrieving trending posts" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] string content, IFormFile? media)
    {
        try
        {
            var userId = GetCurrentUserId();
            var post = await _postService.CreatePost(userId, content, media);

            return Ok(new
            {
                message = "Post created successfully",
                postID = post.PostID,
                mediaType = post.MediaType,
                mediaUrl = post.MediaUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{postId}/like")]
    public async Task<IActionResult> LikePost(long postId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _postService.LikePost(postId, userId);

            if (!result.success)
                return BadRequest(new { message = result.message });

            return Ok(new { message = result.message, likesCount = result.likesCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking post");
            return StatusCode(500, new { message = "Error liking post" });
        }
    }

    [HttpDelete("{postId}/like")]
    public async Task<IActionResult> UnlikePost(long postId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _postService.UnlikePost(postId, userId);

            if (!result.success)
                return BadRequest(new { message = result.message });

            return Ok(new { message = result.message, likesCount = result.likesCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking post");
            return StatusCode(500, new { message = "Error unliking post" });
        }
    }

    [HttpDelete("{postId}")]
    public async Task<IActionResult> DeletePost(long postId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _postService.DeletePost(postId, userId);

            if (!result)
                return NotFound(new { message = "Post not found" });

            return Ok(new { message = "Post deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post");
            return StatusCode(500, new { message = "Error deleting post" });
        }
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        return long.Parse(userIdClaim ?? "0");
    }
}