namespace Users.DataAccess.Entities;

public class User : EntityBase
{
    public string Auth0UserId { get; set; } = null!;
}
