namespace CityPulse.Common.Api.Swagger;

public sealed class SwaggerResponseOptions
{
    private const string NotOpenGenericMessage = "The wrapper must be an open generic type with one type parameter.";

    private readonly HashSet<Type> _wrapperTypes = [];
    private readonly HashSet<Type> _emptyTypes = [];
    private readonly SortedSet<int> _problemStatusCodes = [];

    internal IReadOnlyCollection<int> ProblemStatusCodes => _problemStatusCodes;

    public SwaggerResponseOptions UnwrapResponse(Type openGenericWrapper)
    {
        if (!openGenericWrapper.IsGenericTypeDefinition || openGenericWrapper.GetGenericArguments().Length != 1)
        {
            throw new ArgumentException(NotOpenGenericMessage, nameof(openGenericWrapper));
        }

        _wrapperTypes.Add(openGenericWrapper);

        return this;
    }

    public SwaggerResponseOptions TreatAsEmpty(params Type[] valueTypes)
    {
        foreach (var valueType in valueTypes)
        {
            _emptyTypes.Add(valueType);
        }

        return this;
    }

    public SwaggerResponseOptions DocumentProblems(params int[] statusCodes)
    {
        foreach (var statusCode in statusCodes)
        {
            _problemStatusCodes.Add(statusCode);
        }

        return this;
    }

    internal bool TryUnwrap(Type type, out Type valueType)
    {
        if (type.IsGenericType && _wrapperTypes.Contains(type.GetGenericTypeDefinition()))
        {
            valueType = type.GetGenericArguments()[0];

            return true;
        }

        valueType = type;

        return false;
    }

    internal bool IsEmpty(Type valueType) => _emptyTypes.Contains(valueType);
}
