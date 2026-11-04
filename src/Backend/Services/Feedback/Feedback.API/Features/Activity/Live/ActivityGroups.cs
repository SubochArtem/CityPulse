namespace Feedback.API.Features.Activity.Live;

public static class ActivityGroups
{
    public const string All = "activity:all";

    public static string City(Guid cityId) => $"activity:{cityId}";

    public static string For(Guid? cityId) => cityId.HasValue ? City(cityId.Value) : All;
}
