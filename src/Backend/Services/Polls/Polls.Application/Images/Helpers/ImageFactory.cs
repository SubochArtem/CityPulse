namespace Polls.Application.Images.Helpers;

public delegate TImage ImageFactory<out TImage>(string fileName, int order);
