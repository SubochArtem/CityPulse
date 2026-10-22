using ErrorOr;
using Feedback.API.Common.Services;
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
    private static readonly Error NotFound = Error.NotFound(
        code: "Feedback.NotFound",
        description: "Feedback was not found.");

    private static readonly Error NotAComment = Error.Validation(
        code: "Feedback.NotAComment",
        description: "Only comments can be edited.");

    private static readonly Error NotOwner = Error.Forbidden(
        code: "Feedback.NotOwner",
        description: "You can edit only your own comments.");

    public async Task<ErrorOr<FeedbackResponse>> Handle(
        EditCommentCommand request,
        CancellationToken cancellationToken)
    {
        var item = await context.FeedbackItems
            .Find(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return NotFound;

        if (item.Text is null)
            return NotAComment;

        if (item.UserId != request.UserId)
            return NotOwner;

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
