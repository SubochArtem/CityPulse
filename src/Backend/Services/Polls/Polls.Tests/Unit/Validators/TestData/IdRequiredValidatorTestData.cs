using FluentValidation;
using Polls.Application.Cities.Commands.DeleteCity;
using Polls.Application.Cities.Queries.GetCityById;
using Polls.Application.Cities.Queries.GetCityWithPolls;
using Polls.Application.Common.Constants;
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
            ValidationConstants.City.IdRequired)
    };
}
