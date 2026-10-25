using Xunit;

namespace Polls.Tests.Integration.Grpc.TestData;

public sealed record MalformedIdCase(string Name, string Value)
{
    public override string ToString() => Name;
}

public static class GrpcIntegrationTestData
{
    public static TheoryData<MalformedIdCase> MalformedIds => new()
    {
        new MalformedIdCase("NotAGuid", "not-a-guid"),
        new MalformedIdCase("Digits", "123"),
        new MalformedIdCase("Empty", string.Empty),
        new MalformedIdCase("Whitespace", " ")
    };
}
