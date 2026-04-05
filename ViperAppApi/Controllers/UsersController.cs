using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViperAppApi.Data;
using ViperAppApi.Models.Domains;
using ViperAppApi.Models.DTOs;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ GET PROFILE
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userIdClaim = User.FindFirst("UserId");

        if (userIdClaim == null)
        {
            return Unauthorized("Invalid token");
        }

        var userId = int.Parse(userIdClaim.Value);

        var user = _context.Users.FirstOrDefault(x => x.UserID == userId);

        if (user == null) return NotFound();

        return Ok(user);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "User ID not found in token" });

            var userId = int.Parse(userIdClaim);
            var user = await _context.Users.FirstOrDefaultAsync<User>(x => x.UserID == userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Update text fields only if provided
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

            // ✅ IMAGE UPLOAD
            if (dto.Image != null && dto.Image.Length > 0)
            {
                // Validate image type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(dto.Image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Invalid image format. Only JPG, PNG, GIF are allowed." });
                }

                // Validate file size (max 5MB)
                if (dto.Image.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Image size cannot exceed 5MB" });
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.ProfileImage))
                {
                    var oldImagePath = Path.Combine("wwwroot", user.ProfileImage.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                var fileName = $"{Guid.NewGuid()}{extension}";
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                // Create directory if it doesn't exist
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                var filePath = Path.Combine(imagesPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                // Store the relative path (without wwwroot)
                user.ProfileImage = $"/images/{fileName}";
            }

            await _context.SaveChangesAsync();

            // Return user without sensitive data
            return Ok(new
            {
                user.UserID,
                user.UserName,
                user.Email,
                user.ProfileImage,
                user.Bio,
                user.Website,
                user.Linkedin,
                user.Twitter,
                user.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error updating profile: {ex.Message}" });
        }
    }
}