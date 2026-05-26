namespace Users.Business.Exceptions;

public class CityNotFoundException(Guid cityId)
    : Exception($"City with id '{cityId}' was not found.");
