using FluentValidation;
using Mapster;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.Business.Interfaces;
using Users.DataAccess.Entities;
using Users.DataAccess.Interfaces;

namespace Users.Business.Services;

public class UserService(
    IUserRepository userRepository,
    IIdentityProvider identityProvider,
    IValidator<CreateUserDto> createValidator,
    IValidator<UpdateUserProfileDto> updateValidator,
    ICityService cityService) : IUserService
{
    private readonly IValidator<CreateUserDto> _createValidator = createValidator;
    private readonly IValidator<UpdateUserProfileDto> _updateValidator = updateValidator;
    private readonly IIdentityProvider _identityProvider = identityProvider;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ICityService _cityService = cityService;

    public async Task<GetUserDto> CreateUserAsync(
        CreateUserDto createUserDto,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(
            createUserDto,
            cancellationToken);

        var existingUser = await _userRepository.GetByIdentityIdAsync(
            createUserDto.IdentityId,
            cancellationToken);

        if (existingUser is not null)
            throw new UserAlreadyExistsException(
                createUserDto.IdentityId,
                IdentitySources.Auth0);

        var user = createUserDto.Adapt<User>();

        await _userRepository.CreateAsync(
            user,
            cancellationToken);

        return user.Adapt<GetUserDto>();
    }
    
    public async Task<GetUserDto> UpdateUserAsync(
        Guid id,
        UpdateUserProfileDto updateUserProfileDto,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(
            updateUserProfileDto,
            cancellationToken);

        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);

        if (updateUserProfileDto.CityId is not null)
        {
            var city = await _cityService.GetCityAsync(
                updateUserProfileDto.CityId.Value,
                cancellationToken);

            if (city is null)
                throw new CityNotFoundException(updateUserProfileDto.CityId.Value);

            if (city.Status != CityStatus.Active)
                throw new CityNotActiveException(updateUserProfileDto.CityId.Value);

            user.CityId = updateUserProfileDto.CityId;
        }

        if (updateUserProfileDto.Nickname is not null)
        {
            user.Nickname = updateUserProfileDto.Nickname;
            await _identityProvider.UpdateUserProfileAsync(
                user.IdentityId,
                updateUserProfileDto,
                cancellationToken);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        return user.Adapt<GetUserDto>();
    }

    public async Task DeactivateUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);
        await _identityProvider.BlockUserAsync(user.IdentityId, cancellationToken);
    }

    public async Task ActivateUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);
        await _identityProvider.UnblockUserAsync(user.IdentityId, cancellationToken);
    }

    public async Task DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);
        await _identityProvider.DeleteUserAsync(user.IdentityId, cancellationToken);
        await _userRepository.DeleteAsync(user, cancellationToken);
    }

    public async Task<GetUserDto> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new UserNotFoundException(id.ToString(), IdentitySources.Internal);

        return user.Adapt<GetUserDto>();
    }

    public async Task<GetUserDto?> GetUserByIdentityIdAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdentityIdAsync(identityId, cancellationToken);
        return user?.Adapt<GetUserDto>();
    }

    public async Task<IEnumerable<GetUserDto>> GetAllUsersAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Adapt<IEnumerable<GetUserDto>>();
    }

    private async Task<User> GetExistingUserAsync(
        Guid id,
        string identitySource,
        CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(id, cancellationToken)
               ?? throw new UserNotFoundException(id.ToString(), identitySource);
    }
}
