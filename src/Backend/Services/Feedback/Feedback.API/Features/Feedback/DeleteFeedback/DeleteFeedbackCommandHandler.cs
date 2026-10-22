using ErrorOr;
using Feedback.API.Common.Errors;
using Feedback.API.Infrastructure.Mongo;
using MediatR;
using MongoDB.Driver;

namespace Feedback.API.Features.Feedback.DeleteFeedback;

public class DeleteFeedbackCommandHandler(MongoDbContext context)
    : IRequestHandler<DeleteFeedbackCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.CanDeleteAny && !request.CanDeleteOwn)
            return FeedbackErrors.DeleteNotPermitted;

        var item = await context.FeedbackItems
            .Find(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return FeedbackErrors.NotFound;

        if (!request.CanDeleteAny && item.UserId != request.UserId)
            return FeedbackErrors.CannotDeleteOthers;

        await context.FeedbackItems.DeleteOneAsync(
            x => x.Id == item.Id,
            cancellationToken);

        return Result.Deleted;
    }
}
