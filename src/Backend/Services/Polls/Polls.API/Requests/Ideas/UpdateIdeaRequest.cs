namespace Polls.API.Requests.Ideas;

public record UpdateIdeaRequest(
    string Title,
    string? Description);
