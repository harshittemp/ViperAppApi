using Microsoft.EntityFrameworkCore;
using ViperAppApi.Data;
using ViperAppApi.Models.Domains;
using ViperAppApi.Models.DTOs;

namespace ViperAppApi.Services;

public class PostService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PostService> _logger;

    public PostService(AppDbContext context, IWebHostEnvironment environment, ILogger<PostService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    public async Task<List<PostDto>> GetAllPosts(long currentUserId)
    {
        return await _context.Posts
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostDto
            {
                PostID = p.PostID,
                UserID = p.UserID,
                UserName = p.User.UserName,
                UserImage = p.User.ProfileImage,
                Content = p.Content,
                MediaUrl = p.MediaUrl,
                MediaType = p.MediaType,
                LikesCount = p.LikesCount,
                CommentsCount = p.CommentsCount,
                SharesCount = p.SharesCount,
                CreatedAt = p.CreatedAt,
                IsLiked = _context.PostLikes.Any(pl => pl.PostID == p.PostID && pl.UserID == currentUserId)
            })
            .ToListAsync();
    }

    public async Task<List<PostDto>> GetUserPosts(long userId)
    {
        return await _context.Posts
            .Where(p => p.UserID == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostDto
            {
                PostID = p.PostID,
                UserID = p.UserID,
                UserName = p.User.UserName,
                UserImage = p.User.ProfileImage,
                Content = p.Content,
                MediaUrl = p.MediaUrl,
                MediaType = p.MediaType,
                LikesCount = p.LikesCount,
                CommentsCount = p.CommentsCount,
                SharesCount = p.SharesCount,
                CreatedAt = p.CreatedAt,
                IsLiked = _context.PostLikes.Any(pl => pl.PostID == p.PostID && pl.UserID == userId)
            })
            .ToListAsync();
    }

    public async Task<List<PostDto>> GetTrendingPosts(long currentUserId)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Where(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .OrderByDescending(p => p.LikesCount)
            .ThenByDescending(p => p.CreatedAt)
            .Take(20)
            .Select(p => new PostDto
            {
                PostID = p.PostID,
                UserID = p.UserID,
                UserName = p.User.UserName,
                UserImage = p.User.ProfileImage,
                Content = p.Content,
                MediaUrl = p.MediaUrl,
                MediaType = p.MediaType,
                LikesCount = p.LikesCount,
                CommentsCount = p.CommentsCount,
                SharesCount = p.SharesCount,
                CreatedAt = p.CreatedAt,
                IsLiked = _context.PostLikes.Any(pl => pl.PostID == p.PostID && pl.UserID == currentUserId)
            })
            .ToListAsync();
    }

    public async Task<Post> CreatePost(long userId, string content, IFormFile? media)
    {
        var post = new Post
        {
            UserID = userId,
            Content = string.IsNullOrEmpty(content) ? "" : content,
            CreatedAt = DateTime.UtcNow,
            LikesCount = 0,
            CommentsCount = 0,
            SharesCount = 0
        };

        if (media != null && media.Length > 0)
        {
            _logger.LogInformation($"Processing media: {media.FileName}, Type: {media.ContentType}, Size: {media.Length}");

            var result = await SaveMedia(media);
            if (result.success)
            {
                post.MediaUrl = result.filePath;
                post.MediaType = result.mediaType;
                _logger.LogInformation($"Media saved - URL: {post.MediaUrl}, Type: {post.MediaType}");
            }
            else
            {
                _logger.LogError($"Failed to save media: {result.error}");
                throw new Exception(result.error);
            }
        }

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Post created - ID: {post.PostID}, MediaType: {post.MediaType}");
        return post;
    }

    public async Task<(bool success, string message, int likesCount)> LikePost(long postId, long userId)
    {
        var existingLike = await _context.PostLikes
            .FirstOrDefaultAsync(pl => pl.PostID == postId && pl.UserID == userId);

        if (existingLike != null)
            return (false, "Already liked", 0);

        var like = new PostLike
        {
            PostID = postId,
            UserID = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.PostLikes.Add(like);

        var post = await _context.Posts.FindAsync(postId);
        if (post != null)
            post.LikesCount++;

        await _context.SaveChangesAsync();

        return (true, "Post liked", post?.LikesCount ?? 0);
    }

    public async Task<(bool success, string message, int likesCount)> UnlikePost(long postId, long userId)
    {
        var like = await _context.PostLikes
            .FirstOrDefaultAsync(pl => pl.PostID == postId && pl.UserID == userId);

        if (like == null)
            return (false, "Not liked yet", 0);

        _context.PostLikes.Remove(like);

        var post = await _context.Posts.FindAsync(postId);
        if (post != null && post.LikesCount > 0)
            post.LikesCount--;

        await _context.SaveChangesAsync();

        return (true, "Post unliked", post?.LikesCount ?? 0);
    }

    public async Task<bool> DeletePost(long postId, long userId)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostID == postId && p.UserID == userId);

        if (post == null)
            return false;

        // Delete media file if exists
        if (!string.IsNullOrEmpty(post.MediaUrl))
        {
            var filePath = Path.Combine(_environment.WebRootPath, post.MediaUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                _logger.LogInformation($"Deleted media file: {filePath}");
            }
        }

        // Delete associated likes
        var likes = _context.PostLikes.Where(pl => pl.PostID == postId);
        _context.PostLikes.RemoveRange(likes);

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<(bool success, string filePath, string mediaType, string error)> SaveMedia(IFormFile media)
    {
        try
        {
            // Determine media type
            var isVideo = media.ContentType.StartsWith("video/");
            var mediaType = isVideo ? "video" : "image";
            var maxSize = isVideo ? 100 * 1024 * 1024 : 10 * 1024 * 1024;

            // Validate file size
            if (media.Length > maxSize)
            {
                return (false, "", "", $"File size too large. Max {(maxSize / 1024 / 1024)}MB");
            }

            // Validate file type
            var isValidType = media.ContentType.StartsWith("image/") || media.ContentType.StartsWith("video/");
            if (!isValidType)
            {
                return (false, "", "", "Only image and video files are allowed");
            }

            // Generate unique filename
            var extension = Path.GetExtension(media.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadFolder = mediaType == "image" ? "images" : "videos";

            // Ensure wwwroot exists
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(webRootPath))
                    Directory.CreateDirectory(webRootPath);
            }

            var uploadPath = Path.Combine(webRootPath, "uploads", uploadFolder);

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await media.CopyToAsync(stream);
            }
            var urlPath = $"/uploads/{uploadFolder}/{fileName}";
            _logger.LogInformation($"File saved: {filePath}, URL: {urlPath}");

            return (true, urlPath, mediaType, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving media");
            return (false, "", "", ex.Message);
        }
    }
}