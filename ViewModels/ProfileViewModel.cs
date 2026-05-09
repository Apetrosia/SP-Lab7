using GreenSwampApp.ViewModels;

namespace GreenSwampApp.ViewModels
{
    public class ProfileViewModel
    {
        public UserViewModel User { get; set; }
        public string Bio { get; set; }
        public string CoverImageUrl { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public IEnumerable<PostViewModel> Posts { get; set; }
        public IEnumerable<TrendingTagViewModel> TrendingTags { get; set; }
    }

    public class PostDetailViewModel
    {
        public PostViewModel Post { get; set; }
        public IEnumerable<CommentViewModel> Comments { get; set; }
        public IEnumerable<TrendingTagViewModel> TrendingTags { get; set; }
    }

    public class CommentViewModel
    {
        public UserViewModel User { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; }
    }
}