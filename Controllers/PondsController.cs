using GreenSwampApp.Models;
using GreenSwampApp.Data;
using GreenSwampApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Greenswamp.Controllers
{
    [Route("ponds")]
    public class PondsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PondsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{tag}")]
        public async Task<IActionResult> Index(string tag)
        {
            var tagEntity = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name == tag);

            if (tagEntity == null)
            {
                return NotFound();
            }

            var posts = await _context.PostTags
                .Include(pt => pt.Post)
                    .ThenInclude(p => p.User)
                .Include(pt => pt.Post)
                    .ThenInclude(p => p.Interactions)
                .Include(pt => pt.Post)
                    .ThenInclude(p => p.PostTags)
                        .ThenInclude(p => p.Tag)
                .Where(pt => pt.TagId == tagEntity.TagId)
                .OrderByDescending(pt => pt.Post.CreatedAt)
                .Select(pt => pt.Post)
                .ToListAsync();

            var postViewModels = posts.Select(p => new PostViewModel
            {
                PostId = p.PostId,
                User = new UserViewModel
                {
                    UserId = p.User.UserId,
                    Username = p.User.Username,
                    DisplayName = p.User.DisplayName,
                    AvatarUrl = p.User.AvatarUrl ?? $"https://i.pravatar.cc/100?u={p.User.Username}@greenswamp.com"
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
            });

            var trendingTags = await GetTrendingTagsAsync();

            ViewBag.CurrentTag = tag;
            ViewBag.Posts = postViewModels;
            ViewBag.TrendingTags = trendingTags;

            return View();
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

        private async Task<IEnumerable<TrendingTagViewModel>> GetTrendingTagsAsync()
        {
            return await _context.PostTags
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
        }
    }
}