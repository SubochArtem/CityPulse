namespace CityPulse.Contracts.Auth.Claims;

public class CityPulseClaims
{
    private const string Namespace = "https://city-pulse.com";

    public const string CityId = $"{Namespace}/city_id";
    public const string InternalUserId = $"{Namespace}/internal_user_id";
}
