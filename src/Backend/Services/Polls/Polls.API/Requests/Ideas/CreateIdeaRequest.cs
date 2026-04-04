namespace Polls.API.Requests.Ideas;

public record CreateIdeaRequest(
    string Title,
    string? Description);
