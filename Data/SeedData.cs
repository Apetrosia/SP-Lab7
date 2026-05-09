using GreenSwampApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenSwampApp.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext dbContext)
    {
        var frogUser = GetOrCreateUser(
            dbContext,
            username: "frogmaster",
            email: "frogmaster@greenswamp.local",
            displayName: "Frog Master",
            bio: "I study swamp frogs and write about them.",
            avatarUrl: "https://i.pravatar.cc/150?img=12");

        var chessUser = GetOrCreateUser(
            dbContext,
            username: "chessbishop",
            email: "chessbishop@greenswamp.local",
            displayName: "Chess Bishop",
            bio: "I post chess tactics and tournament notes.",
            avatarUrl: "https://i.pravatar.cc/150?img=32");

        dbContext.SaveChanges();

        var tagFrogs = GetOrCreateTag(dbContext, "frogs");
        var tagSwamp = GetOrCreateTag(dbContext, "swamp");
        var tagChess = GetOrCreateTag(dbContext, "chess");
        var tagTactics = GetOrCreateTag(dbContext, "tactics");

        dbContext.SaveChanges();

        var seedItems = new List<(string Content, long UserId, DateTime CreatedAt, string MediaType, string MediaUrl, string[] Tags)>
        {
            (
                "Morning in the swamp: 12 tree frogs were active after the rain. #frogs #swamp",
                frogUser.UserId,
                DateTime.UtcNow.AddMinutes(-45),
                "none",
                string.Empty,
                ["frogs", "swamp"]
            ),
            (
                "Fun fact: many frogs can absorb water directly through their skin. #frogs",
                frogUser.UserId,
                DateTime.UtcNow.AddMinutes(-30),
                "none",
                string.Empty,
                ["frogs"]
            ),
            (
                "Puzzle of the day: look for a discovered attack in this middlegame setup. #chess #tactics",
                chessUser.UserId,
                DateTime.UtcNow.AddMinutes(-20),
                "none",
                string.Empty,
                ["chess", "tactics"]
            ),
            (
                "Rapid games improve intuition, classical games improve calculation. Both matter. #chess",
                chessUser.UserId,
                DateTime.UtcNow.AddMinutes(-10),
                "none",
                string.Empty,
                ["chess"]
            ),
            (
                "Found a bright green frog near the reeds today. Markings looked almost geometric. #frogs #swamp",
                frogUser.UserId,
                DateTime.UtcNow.AddMinutes(-8),
                "image",
                "https://images.unsplash.com/photo-1456926631375-92c8ce872def?auto=format&fit=crop&w=1200&q=80",
                ["frogs", "swamp"]
            ),
            (
                "Board snapshot from blitz training: knight outpost won the endgame. #chess #tactics",
                chessUser.UserId,
                DateTime.UtcNow.AddMinutes(-5),
                "image",
                "https://images.unsplash.com/photo-1528819622765-d6bcf132f793?auto=format&fit=crop&w=1200&q=80",
                ["chess"]
            )
        };

        var eventSeedItems = new List<(
            string PostContent,
            long UserId,
            DateTime PostCreatedAt,
            string MediaType,
            string MediaUrl,
            string[] Tags,
            string Title,
            string Description,
            string Location,
            DateTime StartTime,
            DateTime? EndTime)>
        {
            (
                "Swamp Night Watch this Friday. Bring flashlights and rubber boots. #frogs #swamp",
                frogUser.UserId,
                DateTime.UtcNow.AddMinutes(-41),
                "image",
                "https://images.unsplash.com/photo-1464965911861-746a04b4bca6?auto=format&fit=crop&w=1200&q=80",
                ["frogs", "swamp"],
                "Swamp Night Watch",
                "Guided evening walk to observe tree frogs and record calls.",
                "North Marsh Boardwalk",
                DateTime.UtcNow.AddDays(2).AddHours(1),
                DateTime.UtcNow.AddDays(2).AddHours(3)
            ),
            (
                "Open tactics meetup: solve puzzles and review candidate moves. #chess #tactics",
                chessUser.UserId,
                DateTime.UtcNow.AddMinutes(-17),
                "none",
                string.Empty,
                ["chess", "tactics"],
                "Tactics Sprint Meetup",
                "45-minute tactics sprint plus short analysis of key positions.",
                "Community Pond Cafe",
                DateTime.UtcNow.AddDays(1).AddHours(4),
                DateTime.UtcNow.AddDays(1).AddHours(5)
            ),
            (
                "Weekend pond cleanup and frog habitat check-in. Everyone is welcome. #swamp",
                frogUser.UserId,
                DateTime.UtcNow.AddMinutes(-9),
                "none",
                string.Empty,
                ["swamp"],
                "Pond Cleanup Day",
                "Volunteer cleanup, habitat notes, and a short safety briefing.",
                "East Pond Pier",
                DateTime.UtcNow.AddDays(4).AddHours(2),
                DateTime.UtcNow.AddDays(4).AddHours(6)
            )
        };

        var allTags = dbContext.Tags.ToDictionary(t => t.Name, t => t);
        var addedPosts = new List<(Post Post, string[] Tags)>();

        var seededContents = seedItems.Select(item => item.Content)
            .Concat(eventSeedItems.Select(item => item.PostContent))
            .ToList();
        var existingSeededPosts = dbContext.Posts
            .Where(p => seededContents.Contains(p.Content))
            .ToDictionary(p => p.Content, p => p);

        foreach (var item in seedItems)
        {
            if (existingSeededPosts.TryGetValue(item.Content, out var existingPost))
            {
                existingPost.UserId = item.UserId;
                existingPost.MediaUrl = item.MediaUrl;
                existingPost.MediaType = item.MediaType;
                existingPost.CreatedAt = item.CreatedAt;
                existingPost.UpdatedAt = item.CreatedAt;
                continue;
            }

            var post = new Post
            {
                UserId = item.UserId,
                Content = item.Content,
                MediaUrl = item.MediaUrl,
                MediaType = item.MediaType,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.CreatedAt
            };

            dbContext.Posts.Add(post);
            addedPosts.Add((post, item.Tags));
            existingSeededPosts[item.Content] = post;
        }

        foreach (var eventItem in eventSeedItems)
        {
            if (existingSeededPosts.TryGetValue(eventItem.PostContent, out var existingEventPost))
            {
                existingEventPost.UserId = eventItem.UserId;
                existingEventPost.MediaUrl = eventItem.MediaUrl;
                existingEventPost.MediaType = eventItem.MediaType;
                existingEventPost.CreatedAt = eventItem.PostCreatedAt;
                existingEventPost.UpdatedAt = eventItem.PostCreatedAt;
                continue;
            }

            var eventPost = new Post
            {
                UserId = eventItem.UserId,
                Content = eventItem.PostContent,
                MediaUrl = eventItem.MediaUrl,
                MediaType = eventItem.MediaType,
                CreatedAt = eventItem.PostCreatedAt,
                UpdatedAt = eventItem.PostCreatedAt
            };

            dbContext.Posts.Add(eventPost);
            addedPosts.Add((eventPost, eventItem.Tags));
            existingSeededPosts[eventItem.PostContent] = eventPost;
        }

        dbContext.SaveChanges();

        var seededPosts = dbContext.Posts
            .Where(p => seededContents.Contains(p.Content))
            .ToList();

        var eventPostContents = eventSeedItems.Select(item => item.PostContent).ToList();
        var eventPostsByContent = dbContext.Posts
            .Where(p => eventPostContents.Contains(p.Content))
            .ToDictionary(p => p.Content, p => p);

        var existingEventsByPostId = dbContext.Events
            .ToDictionary(e => e.PostId, e => e);

        foreach (var eventItem in eventSeedItems)
        {
            if (!eventPostsByContent.TryGetValue(eventItem.PostContent, out var eventPost))
            {
                continue;
            }

            if (existingEventsByPostId.TryGetValue(eventPost.PostId, out var existingEvent))
            {
                existingEvent.UserId = eventItem.UserId;
                existingEvent.Title = eventItem.Title;
                existingEvent.Description = eventItem.Description;
                existingEvent.Location = eventItem.Location;
                existingEvent.StartTime = eventItem.StartTime;
                existingEvent.EndTime = eventItem.EndTime;
                existingEvent.CreatedAt = eventItem.PostCreatedAt;
                continue;
            }

            dbContext.Events.Add(new Event
            {
                PostId = eventPost.PostId,
                UserId = eventItem.UserId,
                Title = eventItem.Title,
                Description = eventItem.Description,
                Location = eventItem.Location,
                StartTime = eventItem.StartTime,
                EndTime = eventItem.EndTime,
                CreatedAt = eventItem.PostCreatedAt
            });
        }

        dbContext.SaveChanges();

        var existingInteractions = dbContext.Interactions
            .AsNoTracking()
            .Select(i => new { i.PostId, i.UserId, i.InteractionType })
            .ToHashSet();

        var interactions = new List<Interaction>();
        var commentTexts = new[]
        {
            "Great post! Love this.",
            "Very insightful, thanks for sharing!",
            "This is exactly what I needed to see.",
            "Amazing perspective on this!",
            "Couldn't agree more with this.",
            "Thanks for the valuable insight."
        };
        var commentIndex = 0;

        for (int postIdx = 0; postIdx < seededPosts.Count; postIdx++)
        {
            var post = seededPosts[postIdx];
            
            // Decide comment count: 1 for even indices, 2 for odd
            var commentCount = postIdx % 2 == 0 ? 1 : 2;
            
            // Decide reribb count: cycle through 0, 2, 1, 3, 1, 2
            var reribbCount = new[] { 0, 2, 1, 3, 1, 2 }[postIdx % 6];
            
            // Add comments from alternating users
            for (int i = 0; i < commentCount; i++)
            {
                var commenterUserId = i % 2 == 0 ? 
                    (post.UserId == frogUser.UserId ? chessUser.UserId : frogUser.UserId) :
                    (post.UserId == frogUser.UserId ? frogUser.UserId : chessUser.UserId);

                var commentKey = new { PostId = post.PostId, UserId = commenterUserId, InteractionType = "comment" };
                if (!existingInteractions.Contains(commentKey))
                {
                    interactions.Add(new Interaction
                    {
                        PostId = post.PostId,
                        UserId = commenterUserId,
                        InteractionType = "comment",
                        CommentContent = commentTexts[commentIndex % commentTexts.Length],
                        CreatedAt = post.CreatedAt.AddMinutes(5 + i * 2)
                    });
                    commentIndex++;
                }
            }

            // Add multiple reribbs from different users
            for (int i = 0; i < reribbCount; i++)
            {
                var reribbUserId = i % 2 == 0 ? 
                    (post.UserId == frogUser.UserId ? chessUser.UserId : frogUser.UserId) :
                    (post.UserId == frogUser.UserId ? frogUser.UserId : chessUser.UserId);

                var reribbKey = new { PostId = post.PostId, UserId = reribbUserId, InteractionType = "reribb" };
                if (!existingInteractions.Contains(reribbKey))
                {
                    interactions.Add(new Interaction
                    {
                        PostId = post.PostId,
                        UserId = reribbUserId,
                        InteractionType = "reribb",
                        CommentContent = string.Empty,
                        CreatedAt = post.CreatedAt.AddMinutes(10 + i)
                    });
                }
            }
        }

        if (interactions.Count > 0)
        {
            dbContext.Interactions.AddRange(interactions);
            dbContext.SaveChanges();
        }

        var postTags = new List<PostTag>();
        foreach (var added in addedPosts)
        {
            foreach (var tagName in added.Tags)
            {
                postTags.Add(new PostTag
                {
                    PostId = added.Post.PostId,
                    TagId = allTags[tagName].TagId
                });
            }
        }

        if (postTags.Count == 0)
        {
            return;
        }

        dbContext.PostTags.AddRange(postTags);
        dbContext.SaveChanges();
    }

    private static User GetOrCreateUser(
        ApplicationDbContext dbContext,
        string username,
        string email,
        string displayName,
        string bio,
        string avatarUrl)
    {
        var existing = dbContext.Users.FirstOrDefault(u => u.Username == username);
        if (existing != null)
        {
            existing.AvatarUrl = avatarUrl;
            existing.UpdatedAt = DateTime.UtcNow;
            return existing;
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = "seed-password-hash",
            DisplayName = displayName,
            Bio = bio,
            AvatarUrl = avatarUrl,
            CoverImageUrl = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        return user;
    }

    private static Tag GetOrCreateTag(ApplicationDbContext dbContext, string tagName)
    {
        var normalized = tagName.Trim().ToLowerInvariant();
        var existing = dbContext.Tags.FirstOrDefault(t => t.Name == normalized);
        if (existing != null)
        {
            return existing;
        }

        var tag = new Tag
        {
            Name = normalized,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Tags.Add(tag);
        return tag;
    }
}
