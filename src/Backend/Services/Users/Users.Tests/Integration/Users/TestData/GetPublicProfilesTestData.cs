using Users.Business.Constants;
using Xunit;

namespace Users.Tests.Integration.Users.TestData;

public static class GetPublicProfilesTestData
{
    public static string BuildQuery(IEnumerable<Guid> ids) =>
        string.Join('&', ids.Select(id => $"ids={id}"));

    public static TheoryData<string, string> InvalidQueries => new()
    {
        { string.Empty, ValidationConstants.PublicProfiles.IdsRequired },
        {
            BuildQuery(Enumerable.Range(0, ValidationConstants.PublicProfiles.MaxIds + 1).Select(_ => Guid.NewGuid())),
            ValidationConstants.PublicProfiles.TooManyIds
        }
    };

    public static TheoryData<string> NonGuidQueries => new()
    {
        "ids=not-a-guid",
        "ids=123",
        $"ids={Guid.NewGuid()}&ids=not-a-guid"
    };
}
