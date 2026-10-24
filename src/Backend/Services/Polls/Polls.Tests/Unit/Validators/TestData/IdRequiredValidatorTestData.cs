using FluentValidation;
using Polls.Application.Cities.Commands.DeleteCity;
using Polls.Application.Cities.Queries.GetCityById;
using Polls.Application.Cities.Queries.GetCityWithPolls;
using Polls.Application.Common.Constants;
using Polls.Application.Ideas.Commands.DeleteIdea;
using Polls.Application.Ideas.Queries.GetIdeaById;
using Polls.Application.Ideas.Queries.GetIdeaCity;
using Polls.Application.Ideas.Queries.GetIdeaWithPoll;
using Polls.Application.Polls.Commands.DeletePoll;
using Polls.Application.Polls.Queries.GetPollById;
using Polls.Application.Polls.Queries.GetPollWithIdeas;
using Xunit;

namespace Polls.Tests.Unit.Validators.TestData;

public sealed record IdRequiredCase(
    string Name,
    IValidator Validator,
    Func<Guid, object> BuildCommand,
    string ExpectedMessage)
{
    public override string ToString() => Name;
}

public static class IdRequiredValidatorTestData
{
    public static TheoryData<IdRequiredCase> Cases => new()
    {
        new IdRequiredCase(
            nameof(DeleteCityCommandValidator),
            new DeleteCityCommandValidator(),
            id => new DeleteCityCommand(id),
            ValidationConstants.City.IdRequired),
        new IdRequiredCase(
            nameof(GetCityByIdQueryValidator),
            new GetCityByIdQueryValidator(),
            id => new GetCityByIdQuery(id),
            ValidationConstants.City.IdRequired),
        new IdRequiredCase(
            nameof(GetCityWithPollsQueryValidator),
            new GetCityWithPollsQueryValidator(),
            id => new GetCityWithPollsQuery(id),
            ValidationConstants.City.IdRequired),
        new IdRequiredCase(
            nameof(DeleteIdeaCommandValidator),
            new DeleteIdeaCommandValidator(),
            id => new DeleteIdeaCommand(id),
            ValidationConstants.Idea.IdRequired),
        new IdRequiredCase(
            nameof(GetIdeaByIdQueryValidator),
            new GetIdeaByIdQueryValidator(),
            id => new GetIdeaByIdQuery(id),
            ValidationConstants.Idea.IdRequired),
        new IdRequiredCase(
            nameof(GetIdeaCityQueryValidator),
            new GetIdeaCityQueryValidator(),
            id => new GetIdeaCityQuery(id),
            ValidationConstants.Idea.IdRequired),
        new IdRequiredCase(
            nameof(GetIdeaWithPollQueryValidator),
            new GetIdeaWithPollQueryValidator(),
            id => new GetIdeaWithPollQuery(id),
            ValidationConstants.Idea.IdRequired),
        new IdRequiredCase(
            nameof(DeletePollCommandValidator),
            new DeletePollCommandValidator(),
            id => new DeletePollCommand(id),
            ValidationConstants.Poll.IdRequired),
        new IdRequiredCase(
            nameof(GetPollByIdQueryValidator),
            new GetPollByIdQueryValidator(),
            id => new GetPollByIdQuery(id),
            ValidationConstants.Poll.IdRequired),
        new IdRequiredCase(
            nameof(GetPollWithIdeasQueryValidator),
            new GetPollWithIdeasQueryValidator(),
            id => new GetPollWithIdeasQuery(id),
            ValidationConstants.Poll.IdRequired)
    };
}
