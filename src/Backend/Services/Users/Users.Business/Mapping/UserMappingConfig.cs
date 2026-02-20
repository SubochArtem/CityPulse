using Mapster;
using Users.Business.DTOs;
using Users.DataAccess.Entities;

namespace Users.Business.Mapping;

public static class UserMappingConfig
{
    public static void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<User, GetUserDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.IdentityId, src => src.Auth0UserId)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        config.NewConfig<CreateUserDto, User>()
            .Map(dest => dest.Auth0UserId, src => src.IdentityId)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt);
        
    }
}
