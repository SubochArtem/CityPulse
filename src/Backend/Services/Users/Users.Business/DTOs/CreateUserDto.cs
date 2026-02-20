namespace Users.Business.DTOs;

public class CreateUserDto
{
    public required string IdentityId { get; set; }
    
    public Guid? CityId { get; set; }
}
