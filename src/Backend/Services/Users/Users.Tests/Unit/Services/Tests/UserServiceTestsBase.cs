using Mapster;
using MapsterMapper;
using Moq;
using Users.Business.Interfaces;
using Users.Business.Mapping;
using Users.Business.Services;
using Users.Business.Validators;
using Users.DataAccess.Interfaces;

namespace Users.Tests.Unit.Services.Tests;

public abstract class UserServiceTestsBase
{
    protected const string Auth0UnavailableMessage = "Auth0 unavailable";
    protected const string DbUnavailableMessage = "db unavailable";

    protected Mock<IUserRepository> UserRepository { get; } = new(MockBehavior.Strict);
    protected Mock<IIdentityProvider> IdentityProvider { get; } = new(MockBehavior.Strict);
    protected Mock<ICityService> CityService { get; } = new(MockBehavior.Strict);
    protected Mock<IEventPublisher> EventPublisher { get; } = new(MockBehavior.Strict);
    protected UserService Sut { get; }

    protected UserServiceTestsBase()
    {
        var config = new TypeAdapterConfig();
        UserMappingConfig.Configure(config);
        IMapper mapper = new Mapper(config);

        Sut = new UserService(
            UserRepository.Object,
            IdentityProvider.Object,
            new UserValidators(
                new CreateUserValidator(),
                new UpdateUserProfileDtoValidator(),
                new UserFilterDtoValidator()),
            CityService.Object,
            mapper,
            EventPublisher.Object);
    }
}
