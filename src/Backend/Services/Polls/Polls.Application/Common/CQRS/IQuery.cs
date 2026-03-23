using MediatR;
using Polls.Domain.Common;

namespace Polls.Application.Common.CQRS;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
