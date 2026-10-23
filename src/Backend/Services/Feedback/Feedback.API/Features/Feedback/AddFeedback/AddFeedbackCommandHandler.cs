using ErrorOr;
using Feedback.API.Common.Errors;
using Feedback.API.Common.Interfaces;
using Feedback.API.Domain;
using Feedback.API.Infrastructure.Mongo;
using MediatR;
using MongoDB.Driver;

namespace Feedback.API.Features.Feedback.AddFeedback;

public class AddFeedbackCommandHandler(
    MongoDbContext context,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<AddFeedbackCommand, ErrorOr<FeedbackResponse>>
{
    public async Task<ErrorOr<FeedbackResponse>> Handle(
        AddFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Text is null && await LikeExistsAsync(request, cancellationToken))
            return FeedbackErrors.LikeAlreadyExists;

        var item = new FeedbackItem
        {
            UserId = request.UserId,
            IdeaId = request.IdeaId,
            Text = request.Text?.Trim(),
            CreatedAt = dateTimeProvider.UtcNow
        };

        try
        {
            await context.FeedbackItems.InsertOneAsync(item, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return FeedbackErrors.LikeAlreadyExists;
        }

        return FeedbackResponse.FromEntity(item);
    }

    private async Task<bool> LikeExistsAsync(
        AddFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        var filter = Builders<FeedbackItem>.Filter.And(
            Builders<FeedbackItem>.Filter.Eq(x => x.UserId, request.UserId),
            Builders<FeedbackItem>.Filter.Eq(x => x.IdeaId, request.IdeaId),
            Builders<FeedbackItem>.Filter.Eq(x => x.Text, null));

        return await context.FeedbackItems
            .Find(filter)
            .AnyAsync(cancellationToken);
    }
}
