using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Application.Common.Security;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Authorization;
using Polls.Domain.Cities;
using Polls.Domain.Common;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Commands.CreatePoll;

public sealed class CreatePollCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IUserContextService userContext,
    CityAccessPolicy cityAccessPolicy)
    : IRequestHandler<CreatePollCommand, Result<PollDto>>
{
    public async Task<Result<PollDto>> Handle(
        CreatePollCommand command,
        CancellationToken cancellationToken)
    {
        var canCreateAny = userContext.UserPermissions.Contains(Permissions.Polls.CreateAny);

        if (!canCreateAny)
        {
            var accessResult = cityAccessPolicy.Check(command.CityId);
            if (!accessResult.IsSuccess)
                return accessResult.Errors[0];
        }
        
        var city = await unitOfWork.Cities.GetByIdAsync(
            command.CityId,
            cancellationToken);

        if (city is null)
            return CityErrors.NotFound(command.CityId);
        
        var filter = new PollFilter
        {
            CityId = command.CityId,
            Type = command.Type,
            Status = PollStatus.Active
        };

        var activePolls = await unitOfWork.Polls.GetFilteredAsync(
            filter,
            cancellationToken);

        if (activePolls.TotalCount > 0)
            return PollErrors.AlreadyExists(command.CityId);

        var poll = new Poll
        {
            CityId = command.CityId,
            Title = command.Title,
            Description = command.Description,
            EndsAt = command.EndsAt,
            Type = command.Type,
            BudgetAmount = command.BudgetAmount,
            Status = PollStatus.Active
        };

        unitOfWork.Polls.Create(poll);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<PollDto>(poll);
    }
}
