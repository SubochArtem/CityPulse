using ErrorOr;
using Feedback.API.Common.Errors;
using Feedback.API.Common.Interfaces;
using Feedback.API.Domain;
using Feedback.API.Infrastructure.Mongo;
using MediatR;
using MongoDB.Driver;

namespace Feedback.API.Features.Feedback.EditComment;

public class EditCommentCommandHandler(
    MongoDbContext context,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<EditCommentCommand, ErrorOr<FeedbackResponse>>
{
    public async Task<ErrorOr<FeedbackResponse>> Handle(
        EditCommentCommand request,
        CancellationToken cancellationToken)
    {
        var item = await context.FeedbackItems
            .Find(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return FeedbackErrors.NotFound;

        if (item.Text is null)
            return FeedbackErrors.NotAComment;

        if (item.UserId != request.UserId)
            return FeedbackErrors.CannotEditOthers;

        item.Text = request.Text.Trim();
        item.UpdatedAt = dateTimeProvider.UtcNow;

        var update = Builders<FeedbackItem>.Update
            .Set(x => x.Text, item.Text)
            .Set(x => x.UpdatedAt, item.UpdatedAt);

        await context.FeedbackItems.UpdateOneAsync(
            x => x.Id == item.Id,
            update,
            cancellationToken: cancellationToken);

        return FeedbackResponse.FromEntity(item);
    }
}
