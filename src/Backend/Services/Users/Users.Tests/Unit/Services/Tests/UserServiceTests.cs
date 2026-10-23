using CityPulse.Contracts.Events.Users;
using CityPulse.Contracts.Events.Users.Enums;
using CityPulse.Contracts.Querying.Pagination;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Moq;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.Business.Interfaces;
using Users.Business.Mapping;
using Users.Business.Services;
using Users.Business.Validators;
using Users.DataAccess.DTOs;
using Users.DataAccess.Entities;
using Users.DataAccess.Entities.Enums;
using Users.DataAccess.Interfaces;
using Users.DataAccess.Models;
using Users.Tests.TestData;
using Users.Tests.Unit.Services.TestData;
using Xunit;

namespace Users.Tests.Unit.Services.Tests;

public sealed class UserServiceTests
{
    private const string NonExistentIdentityId = "auth0|does-not-exist";
    private const string InvalidIdentityId = "auth0";
    private const string NewNickname = "new_nickname";
    private const string CityName = "Test City";
    private const string CityServiceUnavailableMessage = "city service unavailable";
    private const string Auth0UnavailableMessage = "Auth0 unavailable";
    private const string BusUnavailableMessage = "bus unavailable";
    private const string DbUnavailableMessage = "db unavailable";

    private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
    private readonly Mock<IIdentityProvider> _identityProvider = new(MockBehavior.Strict);
    private readonly Mock<ICityService> _cityService = new(MockBehavior.Strict);
    private readonly Mock<IEventPublisher> _eventPublisher = new(MockBehavior.Strict);
    private readonly IMapper _mapper;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        var config = new TypeAdapterConfig();
        UserMappingConfig.Configure(config);
        _mapper = new Mapper(config);

        _sut = new UserService(
            _userRepository.Object,
            _identityProvider.Object,
            new UserValidators(
                new CreateUserValidator(),
                new UpdateUserProfileDtoValidator(),
                new UserFilterDtoValidator()),
            _cityService.Object,
            _mapper,
            _eventPublisher.Object);
    }

    private static CityDto BuildCity(Guid cityId, CityStatus status) => new()
    {
        Id = cityId,
        Name = CityName,
        Status = status
    };

}
