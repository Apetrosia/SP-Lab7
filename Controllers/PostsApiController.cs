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

        // POST: api/PostsApi/token
        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateToken([FromBody] TokenRequestModel request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // Note: In a real app, verify hashed password instead
            if (user == null || user.PasswordHash != request.Password)
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
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        // GET: api/PostsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Interactions)
                .ToListAsync();
        }

        // GET: api/PostsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(long id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Interactions)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        // PUT: api/PostsApi/5
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(long id, Post post)
        {
            if (id != post.PostId)
            {
                return BadRequest();
            }

            var existingPost = await _context.Posts.FindAsync(id);
            if(existingPost == null) {
                return NotFound();
            }

            var userIdStr = User.FindFirst("userId")?.Value;
            if (userIdStr != null && int.TryParse(userIdStr, out int userId) && existingPost.UserId != userId)
            {
                return Forbid();
            }

            existingPost.Content = post.Content;
            // Removed Title mapping because Post model doesn't have a Title property

            _context.Entry(existingPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PostsApi
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<ActionResult<Post>> PostPost(Post post)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (userIdStr != null && int.TryParse(userIdStr, out int userId))
            {
                post.UserId = userId;
            }

            post.CreatedAt = DateTime.UtcNow;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPost", new { id = post.PostId }, post);
        }

        // DELETE: api/PostsApi/5
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(long id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var userIdStr = User.FindFirst("userId")?.Value;
            if (userIdStr != null && int.TryParse(userIdStr, out int userId) && post.UserId != userId)
            {
                return Forbid();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(long id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }

    public class TokenRequestModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

