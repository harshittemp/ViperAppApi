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

    // ✅ GET COMMENTS FOR A POST
    [HttpGet("{postId}/comments")]
    public async Task<IActionResult> GetComments(long postId)
    {
        try
        {
            var comments = await _postService.GetComments(postId);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for post {PostId}", postId);
            return StatusCode(500, new { message = "Error retrieving comments" });
        }
    }

    // ✅ ADD COMMENT TO POST
    [HttpPost("{postId}/comments")]
    public async Task<IActionResult> AddComment(long postId, [FromBody] AddCommentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var comment = await _postService.AddComment(postId, userId, request.Content);
            return Ok(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to post {PostId}", postId);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ✅ DELETE COMMENT
    [HttpDelete("comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(long commentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _postService.DeleteComment(commentId, userId);

            if (!result)
                return NotFound(new { message = "Comment not found" });

            return Ok(new { message = "Comment deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return StatusCode(500, new { message = "Error deleting comment" });
        }
    }

    // ✅ LIKE POST - Returns updated likes count
    [HttpPost("{postId}/like")]
    public async Task<IActionResult> LikePost(long postId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _postService.LikePost(postId, userId);

            return Ok(new
            {
                success = result.success,
                message = result.message,
                likesCount = result.likesCount,
                liked = result.liked
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking post");
            return StatusCode(500, new { message = "Error liking post" });
        }
    }

    // ✅ UNLIKE POST - Returns updated likes count
    [HttpDelete("{postId}/like")]
    public async Task<IActionResult> UnlikePost(long postId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _postService.UnlikePost(postId, userId);

            return Ok(new
            {
                success = result.success,
                message = result.message,
                likesCount = result.likesCount,
                liked = result.liked
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking post");
            return StatusCode(500, new { message = "Error unliking post" });
        }
    }

    // ✅ SHARE POST - Returns updated shares count
    [HttpPost("{postId}/share")]
    public async Task<IActionResult> SharePost(long postId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _postService.SharePost(postId, userId);

            if (!result.success)
                return NotFound(new { message = "Post not found" });

            return Ok(new
            {
                message = "Post shared successfully",
                sharesCount = result.sharesCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing post");
            return StatusCode(500, new { message = "Error sharing post" });
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

// Request DTO for adding comment
public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
}