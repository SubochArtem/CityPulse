namespace Users.DataAccess.Entities;

public class User : EntityBase
{
    public required string IdentityId { get; set; } 
    public required string Nickname { get; set; } 
}
