namespace Users.Business.Exceptions;

public class CityNotActiveException(Guid cityId)
    : Exception($"City with id '{cityId}' is not active.");
