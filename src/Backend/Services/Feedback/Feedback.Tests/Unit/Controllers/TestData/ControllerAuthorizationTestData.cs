using Feedback.API.Authorization;
using Feedback.API.Features.Feedback.AddFeedback;
using Feedback.API.Features.Feedback.DeleteFeedback;
using Feedback.API.Features.Feedback.EditComment;
using Feedback.API.Features.Feedback.GetFilteredFeedback;
using Xunit;

namespace Feedback.Tests.Unit.Controllers.TestData;

public static class ControllerAuthorizationTestData
{
    public static readonly Type[] AdminControllers =
    [
        typeof(AdminAddFeedbackController),
        typeof(AdminEditCommentController),
        typeof(AdminDeleteFeedbackController)
    ];

    public static readonly Type[] UserControllers =
    [
        typeof(UserAddFeedbackController),
        typeof(UserEditCommentController),
        typeof(UserDeleteFeedbackController),
        typeof(GetFilteredFeedbackController)
    ];

    public static readonly (Type Controller, string Method, string Policy)[] EndpointDefinitions =
    [
        (typeof(UserAddFeedbackController), nameof(UserAddFeedbackController.AddFeedback), Permissions.Feedback.CreateCity),
        (typeof(AdminAddFeedbackController), nameof(AdminAddFeedbackController.AddFeedback), Permissions.Feedback.CreateAny),
        (typeof(UserEditCommentController), nameof(UserEditCommentController.EditComment), Permissions.Feedback.UpdateOwn),
        (typeof(AdminEditCommentController), nameof(AdminEditCommentController.EditComment), Permissions.Feedback.UpdateAny),
        (typeof(UserDeleteFeedbackController), nameof(UserDeleteFeedbackController.DeleteFeedback), Permissions.Feedback.DeleteOwn),
        (typeof(AdminDeleteFeedbackController), nameof(AdminDeleteFeedbackController.DeleteFeedback), Permissions.Feedback.DeleteAny),
        (typeof(GetFilteredFeedbackController), nameof(GetFilteredFeedbackController.GetFilteredFeedback), Permissions.Feedback.ReadAny)
    ];

    public static TheoryData<Type, string, string> Endpoints
    {
        get
        {
            var data = new TheoryData<Type, string, string>();

            foreach (var endpoint in EndpointDefinitions)
                data.Add(endpoint.Controller, endpoint.Method, endpoint.Policy);

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

    public static TheoryData<Type> UserControllerTypes
    {
        get
        {
            var data = new TheoryData<Type>();

            foreach (var controller in UserControllers)
                data.Add(controller);

            return data;
        }
    }

    public static TheoryData<Type> AllControllerTypes
    {
        get
        {
            var data = new TheoryData<Type>();

            foreach (var controller in AdminControllers.Concat(UserControllers))
                data.Add(controller);

            return data;
        }
    }
}
