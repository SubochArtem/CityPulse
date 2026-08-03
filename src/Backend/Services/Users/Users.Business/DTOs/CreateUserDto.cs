namespace Users.Business.DTOs;

public class CreateUserDto
{
    public required string IdentityId { get; set; }
    public required string Nickname { get; set; }
}
