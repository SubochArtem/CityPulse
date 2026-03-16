using System.Linq.Expressions;
using System.Reflection;
using Polls.Domain.Common;

namespace Polls.Application.Common.Behaviors;

internal static class FailureFactory<TResponse> where TResponse : Result
{
    private const string ParameterName = "errors";
    private static readonly Func<Error[], TResponse>? _create;

    static FailureFactory()
    {
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var methodName = nameof(Result<object>.Failure);
            var method = responseType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

            if (method is not null)
            {
                var errorsParam = Expression.Parameter(typeof(Error[]), ParameterName);
                var call = Expression.Call(method, errorsParam);
                _create = Expression.Lambda<Func<Error[], TResponse>>(call, errorsParam).Compile();
            }
        }
    }

    public static Func<Error[], TResponse> Create => _create
                                                     ?? throw new InvalidOperationException(
                                                         $"Type {typeof(TResponse).Name} is not a valid Result<T> for validation failures");
}
