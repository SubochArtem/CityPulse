using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CityPulse.Contracts.Events.Polls;
using CityPulse.Contracts.Events.Polls.Enums;
using MassTransit.Testing;
using Polls.Domain.Authorization;
using Polls.Domain.Ideas.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Integration.Handlers.Ideas;
using Xunit;

namespace Polls.Tests.Integration.Messaging.Tests;

public sealed class ActivityEventsOutboxTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string IdeaTitle = "Night buses";
    private const string IdProperty = "id";

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DeliveryPollInterval = TimeSpan.FromMilliseconds(50);

    private async Task<ITestHarness> StartHarnessAsync()
    {
        var harness = Factory.Services.GetTestHarness();
        await harness.Start();

        return harness;
    }

    private static async Task<bool> WaitForConsumedAsync<T>(ITestHarness harness, Func<T, bool> match)
        where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < DeliveryTimeout)
        {
            if (harness.Consumed.Select<T>(x => match(x.Context.Message)).Any())
                return true;

            await Task.Delay(DeliveryPollInterval);
        }

        return false;
    }

    [Fact]
    public async Task CreateIdea_ValidRequest_PublishesIdeaCreatedEventThroughOutbox()
    {
        var harness = await StartHarnessAsync();
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.City, graph.Poll);
        var userId = Guid.NewGuid();

        var response = await CreateClient([Permissions.Ideas.CreateCity], userId, graph.City.Id)
            .PostAsync(
                $"{IdeaIntegrationHelpers.UserIdeasRoute}/{IdeaIntegrationHelpers.PollsSegment}/{graph.Poll.Id}",
                IdeaIntegrationHelpers.BuildCreateForm(IdeaTitle));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var ideaId = JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement
            .GetProperty(IdProperty)
            .GetGuid();
        Assert.True(await WaitForConsumedAsync<IdeaCreatedEvent>(harness, message =>
            message.IdeaId == ideaId &&
            message.PollId == graph.Poll.Id &&
            message.CityId == graph.City.Id &&
            message.UserId == userId &&
            message.Title == IdeaTitle &&
            message.PollTitle == graph.Poll.Title));
    }

    [Fact]
    public async Task ChangeApprovalStatus_StatusChanges_PublishesApprovalStatusChangedEventThroughOutbox()
    {
        var harness = await StartHarnessAsync();
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await CreateClient([Permissions.Ideas.ChangeApprovalStatusAny])
            .PatchAsJsonAsync(
                $"{IdeaIntegrationHelpers.AdminIdeasRoute}/{graph.Idea.Id}/{IdeaIntegrationHelpers.ApprovalStatusSegment}",
                new { newApprovalStatus = (int)IdeaApprovalStatus.Approved });

        Assert.True(response.IsSuccessStatusCode);
        Assert.True(await WaitForConsumedAsync<IdeaApprovalStatusChangedEvent>(harness, message =>
            message.IdeaId == graph.Idea.Id &&
            message.CityId == graph.City.Id &&
            message.PollTitle == graph.Poll.Title &&
            message.ApprovalStatus == IdeaReviewStatus.Approved));
    }
}
