using FluentValidation.TestHelper;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.Queries.GetIdeas;
using Polls.Domain.Ideas.Enums;
using Polls.Tests.Unit.Validators.TestData;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class IdeaFilterValidatorTests
{
    private readonly IdeaFilterValidator _validator = new();

    [Theory]
    [MemberData(nameof(IdeaFilterValidatorTestData.UnknownAccessStatuses), MemberType = typeof(IdeaFilterValidatorTestData))]
    public void Validate_AccessStatusIsNotDefinedInEnum_ShouldHaveValidationError(IdeaAccessStatus status)
    {
        var filter = new IdeaFilter { AccessStatus = status };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.AccessStatus)
            .WithErrorMessage(ValidationConstants.Idea.InvalidAccessStatus);
    }

    [Theory]
    [MemberData(nameof(IdeaFilterValidatorTestData.ValidAccessStatuses), MemberType = typeof(IdeaFilterValidatorTestData))]
    public void Validate_AccessStatusIsNullOrDefinedInEnum_ShouldNotHaveValidationError(IdeaAccessStatus? status)
    {
        var filter = new IdeaFilter { AccessStatus = status };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.AccessStatus);
    }

    [Theory]
    [MemberData(nameof(IdeaFilterValidatorTestData.UnknownApprovalStatuses), MemberType = typeof(IdeaFilterValidatorTestData))]
    public void Validate_ApprovalStatusIsNotDefinedInEnum_ShouldHaveValidationError(IdeaApprovalStatus status)
    {
        var filter = new IdeaFilter { ApprovalStatus = status };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.ApprovalStatus)
            .WithErrorMessage(ValidationConstants.Idea.InvalidApprovalStatus);
    }

    [Theory]
    [MemberData(nameof(IdeaFilterValidatorTestData.ValidApprovalStatuses), MemberType = typeof(IdeaFilterValidatorTestData))]
    public void Validate_ApprovalStatusIsNullOrDefinedInEnum_ShouldNotHaveValidationError(IdeaApprovalStatus? status)
    {
        var filter = new IdeaFilter { ApprovalStatus = status };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.ApprovalStatus);
    }

    [Theory]
    [MemberData(nameof(IdeaFilterValidatorTestData.PollIds), MemberType = typeof(IdeaFilterValidatorTestData))]
    public void Validate_PollIdHasAnyValue_ShouldNotHaveValidationError(Guid? pollId)
    {
        var filter = new IdeaFilter { PollId = pollId };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.PollId);
    }

    [Fact]
    public void Validate_PageIsBelowMinimum_ShouldHaveValidationErrorFromBaseFilterRules()
    {
        var filter = new IdeaFilter { Page = ValidationConstants.Pagination.MinPage - 1 };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage(ValidationConstants.Pagination.PageInvalid);
    }

    [Fact]
    public void Validate_PageSizeIsAboveMaximum_ShouldHaveValidationErrorFromBaseFilterRules()
    {
        var filter = new IdeaFilter { PageSize = ValidationConstants.Pagination.MaxPageSize + 1 };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(ValidationConstants.Pagination.PageSizeTooLarge);
    }

    [Fact]
    public void Validate_DefaultFilter_ShouldNotHaveAnyValidationErrors()
    {
        var result = _validator.TestValidate(new IdeaFilter());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
