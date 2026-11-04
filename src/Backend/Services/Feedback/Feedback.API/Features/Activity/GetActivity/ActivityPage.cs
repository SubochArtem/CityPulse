namespace Feedback.API.Features.Activity.GetActivity;

public record ActivityPage(
    IReadOnlyList<ActivityResponse> Items,
    string? NextCursor);
