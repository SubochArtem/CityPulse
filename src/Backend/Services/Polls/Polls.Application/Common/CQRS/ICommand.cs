using MediatR;
using Polls.Domain.Common;

namespace Polls.Application.Common.CQRS;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

public interface ICommand : IRequest<Result<Unit>>;
