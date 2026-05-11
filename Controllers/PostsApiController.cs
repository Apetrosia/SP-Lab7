using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GreenSwampApp.Data;
using GreenSwampApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GreenSwampApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public PostsApiController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private int? GetUserIdFromToken()
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            return userIdStr != null && int.TryParse(userIdStr, out var userId) ? userId : null;
        }

        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateToken([FromQuery] string username, [FromQuery] string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.PasswordHash != password)
                return Unauthorized();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "superSecretKey@345V$y1Verys3cr3t#"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("userId", user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "GreenSwampIssuer",
                audience: _config["Jwt:Audience"] ?? "GreenSwampAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: credentials);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Interactions)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(long id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Interactions)
                .FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null) return NotFound();
            return post;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<ActionResult<Post>> PostPost([FromQuery] string content, [FromQuery] string? mediaUrl)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var post = new Post
            {
                UserId = userId.Value,
                Content = content,
                MediaUrl = mediaUrl ?? string.Empty,
                MediaType = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, post);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(long id, [FromQuery] string content, [FromQuery] string? mediaUrl)
        {
            var existingPost = await _context.Posts.FindAsync(id);
            if (existingPost == null) return NotFound();

            var userId = GetUserIdFromToken();
            if (userId == null || existingPost.UserId != userId) return Forbid();

            existingPost.Content = content;
            if (mediaUrl != null) existingPost.MediaUrl = mediaUrl;

            _context.Entry(existingPost).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!PostExists(id)) return NotFound(); else throw; }
            return NoContent();
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(long id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var userId = GetUserIdFromToken();
            if (userId == null || post.UserId != userId) return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool PostExists(long id) => _context.Posts.Any(e => e.PostId == id);

        // GET: api/PostsApi/events/{postId}
        [HttpGet("events/{postId}")]
        public async Task<ActionResult<Event>> GetEventByPost(long postId)
        {
            var eventItem = await _context.Events
                .Include(e => e.Post)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.PostId == postId);

            if (eventItem == null) return NotFound();
            return eventItem;
        }

        // POST: api/PostsApi/events
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("events")]
        public async Task<ActionResult<Event>> PostEvent(
            [FromQuery] long postId,
            [FromQuery] string title,
            [FromQuery] string description,
            [FromQuery] DateTime startTime,
            [FromQuery] string? location)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();
            if (post.UserId != userId) return Forbid();

            if (await _context.Events.AnyAsync(e => e.PostId == postId))
                return BadRequest(new { message = "У этого поста уже есть событие" });

            var eventItem = new Event
            {
                PostId = postId,
                UserId = userId.Value,
                Title = title,
                Description = description,
                StartTime = startTime,
                Location = location ?? string.Empty
            };

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEventByPost), new { postId = eventItem.PostId }, eventItem);
        }

        // PUT: api/PostsApi/events/{postId}
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("events/{postId}")]
        public async Task<IActionResult> PutEvent(
            long postId,
            [FromQuery] string title,
            [FromQuery] string description,
            [FromQuery] DateTime startTime,
            [FromQuery] string? location)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var eventItem = await _context.Events.FirstOrDefaultAsync(e => e.PostId == postId);
            if (eventItem == null) return NotFound();

            if (eventItem.UserId != userId) return Forbid();

            eventItem.Title = title;
            eventItem.Description = description;
            eventItem.StartTime = startTime;
            if (location != null) eventItem.Location = location;

            _context.Entry(eventItem).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!EventExists(postId)) return NotFound(); else throw; }
            return NoContent();
        }

        // DELETE: api/PostsApi/events/{postId}
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("events/{postId}")]
        public async Task<IActionResult> DeleteEvent(long postId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var eventItem = await _context.Events.FirstOrDefaultAsync(e => e.PostId == postId);
            if (eventItem == null) return NotFound();

            if (eventItem.UserId != userId) return Forbid();

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool EventExists(long postId) => _context.Events.Any(e => e.PostId == postId);

        // GET: api/PostsApi/interactions/{id}
        [HttpGet("interactions/{id}")]
        public async Task<ActionResult<Interaction>> GetInteraction(long id)
        {
            var interaction = await _context.Interactions
                .Include(i => i.Post)
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.InteractionId == id);

            if (interaction == null) return NotFound();
            return interaction;
        }

        // GET: api/PostsApi/interactions/post/{postId}
        [HttpGet("interactions/post/{postId}")]
        public async Task<ActionResult<IEnumerable<Interaction>>> GetInteractionsByPost(long postId)
        {
            return await _context.Interactions
                .Include(i => i.User)
                .Where(i => i.PostId == postId)
                .ToListAsync();
        }

        // POST: api/PostsApi/interactions
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("interactions")]
        public async Task<ActionResult<Interaction>> PostInteraction(
            [FromQuery] long postId,
            [FromQuery] string type,      // "like", "comment", "view" и т.д.
            [FromQuery] string? content)  // текст комментария (опционально)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();

            var interaction = new Interaction
            {
                PostId = postId,
                UserId = userId.Value,
                InteractionType = type,
                CommentContent = content ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.
                Interactions.Add(interaction);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInteraction), new { id = interaction.InteractionId }, interaction);
        }

        // PUT: api/PostsApi/interactions/{id}
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("interactions/{id}")]
        public async Task<IActionResult> PutInteraction(
            long id,
            [FromQuery] string? content,
            [FromQuery] string? type)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var interaction = await _context.Interactions.FindAsync(id);
            if (interaction == null) return NotFound();

            if (interaction.UserId != userId) return Forbid();

            if (content != null) interaction.CommentContent = content;
            if (type != null) interaction.InteractionType = type;

            _context.Entry(interaction).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!InteractionExists(id)) return NotFound(); else throw; }
            return NoContent();
        }

        // DELETE: api/PostsApi/interactions/{id}
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("interactions/{id}")]
        public async Task<IActionResult> DeleteInteraction(long id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var interaction = await _context.Interactions.FindAsync(id);
            if (interaction == null) return NotFound();

            if (interaction.UserId != userId)
            {
                var post = await _context.Posts.FindAsync(interaction.PostId);
                if (post == null || post.UserId != userId) return Forbid();
            }

            _context.Interactions.Remove(interaction);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool InteractionExists(long id) => _context.Interactions.Any(i => i.InteractionId == id);
    }
}