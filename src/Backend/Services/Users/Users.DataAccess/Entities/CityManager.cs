namespace Users.DataAccess.Entities;

public class CityManager
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int CityId { get; set; }
    public bool IsActive { get; set; } = true;
}