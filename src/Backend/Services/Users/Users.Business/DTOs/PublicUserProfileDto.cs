namespace Users.Business.DTOs;

public class PublicUserProfileDto
{
    public Guid Id { get; set; }
    public required string Nickname { get; set; }
}
