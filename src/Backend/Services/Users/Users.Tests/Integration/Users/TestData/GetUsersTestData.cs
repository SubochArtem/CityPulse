using Xunit;

namespace Users.Tests.Integration.Users.TestData;

public static class GetUsersTestData
{
    public const string AliceSmith = "alice_smith";
    public const string AliceJones = "ALICE_jones";
    public const string BobSmith = "bob_smith";

    public static readonly string[] SeededNicknames = [AliceSmith, AliceJones, BobSmith];

    public static TheoryData<string, string[]> NicknameSearches => new()
    {
        { "alice", [AliceSmith, AliceJones] },
        { "ALICE", [AliceSmith, AliceJones] },
        { "Smith", [AliceSmith, BobSmith] },
        { "bob_s", [BobSmith] },
        { AliceJones, [AliceJones] }
    };
}
