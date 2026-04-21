namespace Polls.Application.Common.Models;

public record ImageChanges(
    IReadOnlyList<ImageFile>? ToAdd = null,
    IReadOnlyList<Guid>? ToDeleteIds = null);
