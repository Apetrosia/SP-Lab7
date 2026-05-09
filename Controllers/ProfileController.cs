using GreenSwampApp.Models;
using GreenSwampApp.Data;
using GreenSwampApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Greenswamp.Controllers
{
    [Route("profile")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> Index(string username)
        {
            var user = await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Interactions)
                .Include(u => u.Posts)
                    .ThenInclude(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }

            var posts = user.Posts
                .Where(p => p.Event == null)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostViewModel
                {
                    PostId = p.PostId,
                    User = new UserViewModel
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        DisplayName = user.DisplayName,
                        AvatarUrl = user.AvatarUrl ?? $"https://i.pravatar.cc/100?u={user.Username}@greenswamp.com"
                    },
                    Content = ParseContentWithHashtags(p.Content),
                    MediaUrl = p.MediaUrl,
                    MediaType = p.MediaType,
                    CreatedAt = p.CreatedAt,
                    TimeAgo = GetTimeAgo(p.CreatedAt),
                    AnswersCount = p.Interactions.Count(i => i.InteractionType == "comment"),
                    ReribbsCount = p.Interactions.Count(i => i.InteractionType == "reribb"),
                    Tags = p.PostTags.Select(pt => new TagViewModel
                    {
                        TagId = pt.Tag.TagId,
                        Name = pt.Tag.Name
                    })
                })
                .ToList();

            var trendingTags = await _context.PostTags
                .Include(pt => pt.Tag)
                .GroupBy(pt => pt.Tag)
                .Select(g => new TrendingTagViewModel
                {
                    Name = g.Key.Name,
                    PostCount = g.Count()
                })
                .OrderByDescending(t => t.PostCount)
                .Take(5)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = new UserViewModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    AvatarUrl = user.AvatarUrl ?? $"https://i.pravatar.cc/200?u={user.Username}@greenswamp.com"
                },
                Bio = user.Bio,
                CoverImageUrl = user.CoverImageUrl,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                Posts = posts,
                TrendingTags = trendingTags
            };

            return View(viewModel);
        }

        private string ParseContentWithHashtags(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;

            return System.Text.RegularExpressions.Regex.Replace(
                content,
                @"#(\w+)",
                "<a href='/ponds/$1' class='text-swamp-600 hover:text-swamp-800 hover:underline'>#$1</a>"
            );
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1) return "just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d";

            return dateTime.ToString("MMM d");
        }
    }
}