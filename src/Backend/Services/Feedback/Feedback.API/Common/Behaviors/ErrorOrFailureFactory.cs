using System.Linq.Expressions;
using System.Reflection;
using ErrorOr;

namespace Feedback.API.Common.Behaviors;

internal static class ErrorOrFailureFactory<TResponse> where TResponse : IErrorOr
{
    private const string FactoryMethodName = "From";
    private const string ParameterName = "errors";

    public static Func<List<Error>, TResponse> Create { get; } = InitializeFactory();

    private static Func<List<Error>, TResponse> InitializeFactory()
    {
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ErrorOr<>))
        {
            var method = responseType.GetMethod(
                FactoryMethodName,
                BindingFlags.Public | BindingFlags.Static,
                [typeof(List<Error>)]);

            if (method is not null)
            {
                var errorsParam = Expression.Parameter(typeof(List<Error>), ParameterName);
                var call = Expression.Call(method, errorsParam);
                return Expression.Lambda<Func<List<Error>, TResponse>>(call, errorsParam).Compile();
            }
        }

        throw new InvalidOperationException(
            $"Type {responseType.Name} is not a valid ErrorOr<T> for validation failures");
    }
}
