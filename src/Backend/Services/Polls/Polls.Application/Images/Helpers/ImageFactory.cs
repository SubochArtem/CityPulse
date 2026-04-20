namespace Polls.Application.Images.Helpers;

public delegate TImage ImageFactory<TImage>(string fileName, int order);
