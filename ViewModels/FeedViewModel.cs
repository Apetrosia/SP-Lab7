using GreenSwampApp.Models;

namespace GreenSwampApp.ViewModels
{
    public class FeedViewModel
    {
        public IEnumerable<FeedItemViewModel> FeedItems { get; set; }
        public IEnumerable<TrendingTagViewModel> TrendingTags { get; set; }
        public IEnumerable<UpcomingEventViewModel> UpcomingEvents { get; set; }
    }

    public class FeedItemViewModel
    {
        public string Type { get; set; }
        public PostViewModel Post { get; set; }
        public EventViewModel Event { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PostViewModel
    {
        public long PostId { get; set; }
        public UserViewModel User { get; set; }
        public string Content { get; set; }
        public string MediaUrl { get; set; }
        public string MediaType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; }
        public int AnswersCount { get; set; }
        public int ReribbsCount { get; set; }
        public IEnumerable<TagViewModel> Tags { get; set; }
    }

    public class EventViewModel
    {
        public long EventId { get; set; }
        public long PostId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string MediaUrl { get; set; }
        public string MediaType { get; set; }
        public UserViewModel Host { get; set; }
    }

    public class UserViewModel
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class TagViewModel
    {
        public long TagId { get; set; }
        public string Name { get; set; }
    }

    public class TrendingTagViewModel
    {
        public string Name { get; set; }
        public int PostCount { get; set; }
    }

    public class UpcomingEventViewModel
    {
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public string Location { get; set; }
    }
}