namespace Users.DataAccess.Models;

public class UserFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    public string? Nickname { get; set; }
    public Guid? CityId { get; set; }
}
