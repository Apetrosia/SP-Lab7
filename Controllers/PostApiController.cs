using GreenSwampApp.Data;
using GreenSwampApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace GreenSwampApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class CreatePostRequest
        {
            public string Content { get; set; }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Content cannot be empty.");

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out var userId))
                return Unauthorized();

            // Extract tags
            var hashtags = ExtractHashtags(request.Content);

            // Highlight tags in content (simple approach)
            var highlightedContent = Regex.Replace(
                request.Content,
                @"(#\w+)",
                "<span class=\"text-swamp-600 hover:text-swamp-800 hover:underline cursor-pointer\">$1</span>"
            );

            var post = new Post
            {
                UserId = userId,
                Content = highlightedContent,
                MediaUrl = string.Empty,
                MediaType = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync(); // Get PostId

            // Process tags
            foreach (var tagStr in hashtags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tagStr.ToLower());
                if (tag == null)
                {
                    tag = new Tag { Name = tagStr, CreatedAt = DateTime.UtcNow };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                _context.PostTags.Add(new PostTag
                {
                    PostId = post.PostId,
                    TagId = tag.TagId
                });
            }

            if (hashtags.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, postId = post.PostId });
        }

        public class CreateInteractionRequest
        {
            public long PostId { get; set; }
            public string InteractionType { get; set; } // 'reribb', 'comment'
            public string? CommentContent { get; set; }
        }

        [HttpPost("interaction")]
        [Authorize]
        public async Task<IActionResult> AddInteraction([FromBody] CreateInteractionRequest request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out var userId))
                return Unauthorized();

            var interaction = new Interaction
            {
                UserId = userId,
                PostId = request.PostId,
                InteractionType = request.InteractionType,
                CommentContent = request.CommentContent ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.Interactions.Add(interaction);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        private List<string> ExtractHashtags(string content)
        {
            var regex = new Regex(@"(#\w+)");
            var matches = regex.Matches(content);
            return matches.Select(m => m.Value).Distinct().ToList();
        }
    }
}
