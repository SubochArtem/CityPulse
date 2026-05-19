namespace Users.Business.DTOs;

public class GetUserDto
{
    public Guid Id { get; set; }
    
    public required string IdentityId { get; set; }
    
    public required string Nickname { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Guid? CityId { get; set; }
}
