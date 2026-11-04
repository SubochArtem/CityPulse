namespace Feedback.API.Features.Activity;

public static class ActivityKeys
{
    public static string PollOpened(Guid pollId) => $"poll-opened:{pollId}";

    public static string PollClosed(Guid pollId, string status) => $"poll-closed:{pollId}:{status}";

    public static string IdeaProposed(Guid ideaId) => $"idea-proposed:{ideaId}";

    public static string IdeaReviewed(Guid ideaId, string status) => $"idea-reviewed:{ideaId}:{status}";

    public static string IdeaCommented(Guid feedbackId) => $"idea-commented:{feedbackId}";
}
