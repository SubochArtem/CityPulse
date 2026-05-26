namespace Users.Business.DTOs;

public class GetUserDto
{
    public Guid Id { get; set; }
    
    public required string IdentityId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public DateTimeOffset? LastSyncedAt { get; set; }
}
