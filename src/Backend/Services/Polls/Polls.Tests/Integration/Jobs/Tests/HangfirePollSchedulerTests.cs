using Hangfire;
using Hangfire.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Polls;
using Polls.Domain.PollScheduleJob;
using Polls.Infrastructure.Jobs;
using Polls.Tests.Infrastructure;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.Integration.Handlers.Polls;
using Xunit;

namespace Polls.Tests.Integration.Jobs.Tests;

public sealed class HangfirePollSchedulerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string FirstJobId = "job-1";
    private const string SecondJobId = "job-2";
    private const string ThirdJobId = "job-3";

    private readonly Mock<IBackgroundJobClient> _client = new();

    private sealed record ScheduledPoll(Poll Poll, PollScheduleJob Job);

    private async Task<ScheduledPoll[]> SeedScheduledPollsAsync(params string[] jobIds)
    {
        var city = CityIntegrationHelpers.BuildCity();
        var scheduled = jobIds
            .Select(jobId =>
            {
                var poll = PollIntegrationHelpers.BuildPoll(city.Id);
                var job = new PollScheduleJob { PollId = poll.Id, HangfireJobId = jobId };

                return new ScheduledPoll(poll, job);
            })
            .ToArray();

        await SeedAsync(city);
        await SeedAsync(scheduled.Select(x => (object)x.Poll).ToArray());
        await SeedAsync(scheduled.Select(x => (object)x.Job).ToArray());

        return scheduled;
    }

    private async Task RunAsync(Func<HangfirePollScheduler, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await action(new HangfirePollScheduler(unitOfWork, _client.Object));
    }

    private Task<string[]> RemainingJobIdsAsync() =>
        QueryDbAsync(context => context.PollScheduleJobs.Select(x => x.HangfireJobId).ToArrayAsync());

    private void VerifyJobDeleted(string jobId, Times times) =>
        _client.Verify(
            x => x.ChangeState(jobId, It.IsAny<DeletedState>(), It.IsAny<string>()),
            times);

    [Fact]
    public async Task CancelRange_SeveralPolls_DeletesTheirJobsOnly()
    {
        var scheduled = await SeedScheduledPollsAsync(FirstJobId, SecondJobId, ThirdJobId);

        await RunAsync(scheduler => scheduler.CancelRangeAsync([scheduled[0].Poll.Id, scheduled[1].Poll.Id]));

        Assert.Equal([ThirdJobId], await RemainingJobIdsAsync());
        VerifyJobDeleted(FirstJobId, Times.Once());
        VerifyJobDeleted(SecondJobId, Times.Once());
        VerifyJobDeleted(ThirdJobId, Times.Never());
    }

    [Fact]
    public async Task CancelRange_SomePollsHaveNoJob_IgnoresThemAndDeletesTheRest()
    {
        var scheduled = await SeedScheduledPollsAsync(FirstJobId, SecondJobId);

        await RunAsync(scheduler => scheduler.CancelRangeAsync([scheduled[0].Poll.Id, Guid.NewGuid()]));

        Assert.Equal([SecondJobId], await RemainingJobIdsAsync());
        VerifyJobDeleted(FirstJobId, Times.Once());
        VerifyJobDeleted(SecondJobId, Times.Never());
    }

    [Fact]
    public async Task CancelRange_NoPollsGiven_DoesNothing()
    {
        await SeedScheduledPollsAsync(FirstJobId);

        await RunAsync(scheduler => scheduler.CancelRangeAsync([]));

        Assert.Equal([FirstJobId], await RemainingJobIdsAsync());
        _client.Verify(
            x => x.ChangeState(It.IsAny<string>(), It.IsAny<IState>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelRange_NoPollHasAJob_DeletesNothing()
    {
        await SeedScheduledPollsAsync(FirstJobId);

        await RunAsync(scheduler => scheduler.CancelRangeAsync([Guid.NewGuid(), Guid.NewGuid()]));

        Assert.Equal([FirstJobId], await RemainingJobIdsAsync());
        _client.Verify(
            x => x.ChangeState(It.IsAny<string>(), It.IsAny<IState>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Cancel_PollHasAJob_DeletesItAndItsRecord()
    {
        var scheduled = await SeedScheduledPollsAsync(FirstJobId, SecondJobId);

        await RunAsync(scheduler => scheduler.CancelAsync(scheduled[0].Poll.Id));

        Assert.Equal([SecondJobId], await RemainingJobIdsAsync());
        VerifyJobDeleted(FirstJobId, Times.Once());
    }

    [Fact]
    public async Task Cancel_PollHasNoJob_DeletesNothing()
    {
        await SeedScheduledPollsAsync(FirstJobId);

        await RunAsync(scheduler => scheduler.CancelAsync(Guid.NewGuid()));

        Assert.Equal([FirstJobId], await RemainingJobIdsAsync());
        _client.Verify(
            x => x.ChangeState(It.IsAny<string>(), It.IsAny<IState>(), It.IsAny<string>()),
            Times.Never);
    }
}
