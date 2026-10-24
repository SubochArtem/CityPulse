using Polls.API.Controllers.Admin;
using Polls.API.Controllers.User;
using Polls.Domain.Authorization;
using Xunit;

namespace Polls.Tests.Unit.Controllers.TestData;

public static class ControllerAuthorizationTestData
{
    public static readonly Type[] AdminControllers =
    [
        typeof(AdminCitiesController),
        typeof(AdminIdeasController),
        typeof(AdminPollsController)
    ];

    public static readonly Type[] UserControllers =
    [
        typeof(CitiesController),
        typeof(IdeasController),
        typeof(PollsController)
    ];

    public static readonly (Type Controller, string Method, string Policy)[] ProtectedEndpointDefinitions =
    [
        (typeof(PollsController), nameof(PollsController.CreatePoll), Permissions.Polls.CreateCity),
        (typeof(PollsController), nameof(PollsController.UpdatePoll), Permissions.Polls.UpdateCity),
        (typeof(PollsController), nameof(PollsController.DeletePoll), Permissions.Polls.DeleteCity),

        (typeof(IdeasController), nameof(IdeasController.CreateIdea), Permissions.Ideas.CreateCity),
        (typeof(IdeasController), nameof(IdeasController.UpdateIdea), Permissions.Ideas.UpdateOwn),
        (typeof(IdeasController), nameof(IdeasController.DeleteIdea), Permissions.Ideas.DeleteOwn),

        (typeof(AdminCitiesController), nameof(AdminCitiesController.GetCities), Permissions.Cities.ReadAny),
        (typeof(AdminCitiesController), nameof(AdminCitiesController.GetCityById), Permissions.Cities.ReadAny),
        (typeof(AdminCitiesController), nameof(AdminCitiesController.GetCityWithPolls), Permissions.Cities.ReadAny),
        (typeof(AdminCitiesController), nameof(AdminCitiesController.CreateCity), Permissions.Cities.CreateAny),
        (typeof(AdminCitiesController), nameof(AdminCitiesController.UpdateCity), Permissions.Cities.UpdateAny),
        (typeof(AdminCitiesController), nameof(AdminCitiesController.DeleteCity), Permissions.Cities.DeleteAny),
        (typeof(AdminCitiesController), nameof(AdminCitiesController.ChangeStatus), Permissions.Cities.ChangeStatusAny),

        (typeof(AdminIdeasController), nameof(AdminIdeasController.GetIdeas), Permissions.Ideas.ReadAny),
        (typeof(AdminIdeasController), nameof(AdminIdeasController.GetIdeaById), Permissions.Ideas.ReadAny),
        (typeof(AdminIdeasController), nameof(AdminIdeasController.GetIdeaWithPoll), Permissions.Ideas.ReadAny),
        (typeof(AdminIdeasController), nameof(AdminIdeasController.CreateIdea), Permissions.Ideas.CreateAny),
        (typeof(AdminIdeasController), nameof(AdminIdeasController.UpdateIdea), Permissions.Ideas.UpdateAny),
        (typeof(AdminIdeasController), nameof(AdminIdeasController.DeleteIdea), Permissions.Ideas.DeleteAny),
        (typeof(AdminIdeasController), nameof(AdminIdeasController.ChangeAccessStatus), Permissions.Ideas.ChangeAccessStatusAny),
        (typeof(AdminIdeasController), nameof(AdminIdeasController.ChangeApprovalStatus), Permissions.Ideas.ChangeApprovalStatusAny),

        (typeof(AdminPollsController), nameof(AdminPollsController.GetPolls), Permissions.Polls.ReadAny),
        (typeof(AdminPollsController), nameof(AdminPollsController.GetPollById), Permissions.Polls.ReadAny),
        (typeof(AdminPollsController), nameof(AdminPollsController.GetPollWithIdeas), Permissions.Polls.ReadAny),
        (typeof(AdminPollsController), nameof(AdminPollsController.CreatePoll), Permissions.Polls.CreateAny),
        (typeof(AdminPollsController), nameof(AdminPollsController.UpdatePoll), Permissions.Polls.UpdateAny),
        (typeof(AdminPollsController), nameof(AdminPollsController.DeletePoll), Permissions.Polls.DeleteAny),
        (typeof(AdminPollsController), nameof(AdminPollsController.ChangeStatus), Permissions.Polls.ChangeStatusAny)
    ];

    public static readonly (Type Controller, string Method)[] PublicReadDefinitions =
    [
        (typeof(CitiesController), nameof(CitiesController.GetCities)),
        (typeof(CitiesController), nameof(CitiesController.GetCityById)),
        (typeof(CitiesController), nameof(CitiesController.GetCityWithPolls)),

        (typeof(IdeasController), nameof(IdeasController.GetIdeas)),
        (typeof(IdeasController), nameof(IdeasController.GetIdeaById)),
        (typeof(IdeasController), nameof(IdeasController.GetIdeaWithPoll)),

        (typeof(PollsController), nameof(PollsController.GetPolls)),
        (typeof(PollsController), nameof(PollsController.GetPollById)),
        (typeof(PollsController), nameof(PollsController.GetPollWithIdeas))
    ];

    public static TheoryData<Type, string, string> ProtectedEndpoints
    {
        get
        {
            var data = new TheoryData<Type, string, string>();

            foreach (var endpoint in ProtectedEndpointDefinitions)
                data.Add(endpoint.Controller, endpoint.Method, endpoint.Policy);

            return data;
        }
    }

    public static TheoryData<Type, string> PublicReads
    {
        get
        {
            var data = new TheoryData<Type, string>();

            foreach (var endpoint in PublicReadDefinitions)
                data.Add(endpoint.Controller, endpoint.Method);

            return data;
        }
    }

    public static TheoryData<Type> AdminControllerTypes
    {
        get
        {
            var data = new TheoryData<Type>();

            foreach (var controller in AdminControllers)
                data.Add(controller);

            return data;
        }
    }
}
