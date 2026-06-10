using FluentValidation;
using MapsterMapper;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.Business.Interfaces;
using Users.Business.Responses;
using Users.DataAccess.Entities;
using Users.DataAccess.Interfaces;
using Users.DataAccess.Models;

namespace Users.Business.Services;

public class UserService(
    IUserRepository userRepository,
    IIdentityProvider identityProvider,
    IValidator<CreateUserDto> createValidator,
    IValidator<UpdateUserProfileDto> updateValidator,
    ICityService cityService,
    IMapper mapper) : IUserService
{
    public async Task<GetUserDto> CreateUserAsync(
        CreateUserDto createUserDto,
        CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(
            createUserDto,
            cancellationToken);

        var existingUser = await userRepository.GetByIdentityIdAsync(
            createUserDto.IdentityId,
            cancellationToken);

        if (existingUser is not null)
            throw new UserAlreadyExistsException(
                createUserDto.IdentityId,
                IdentitySources.Auth0);

        var user = mapper.Map<User>(createUserDto);

        await userRepository.CreateAsync(
            user,
            cancellationToken);

        return mapper.Map<GetUserDto>(user);
    }

    public async Task<GetUserDto> UpdateUserAsync(
        Guid id,
        UpdateUserProfileDto updateUserProfileDto,
        CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(
            updateUserProfileDto,
            cancellationToken);

        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);

        var isAuth0UpdateRequired = false;

        if (updateUserProfileDto.CityId is not null)
        {
            var city = await cityService.GetCityAsync(
                updateUserProfileDto.CityId.Value,
                cancellationToken);

            if (city is null)
                throw new CityNotFoundException(updateUserProfileDto.CityId.Value);

            if (city.Status != CityStatus.Active)
                throw new CityNotActiveException(updateUserProfileDto.CityId.Value);

            user.CityId = updateUserProfileDto.CityId;
            isAuth0UpdateRequired = true;
        }

        if (updateUserProfileDto.Nickname is not null)
        {
            user.Nickname = updateUserProfileDto.Nickname;
            isAuth0UpdateRequired = true;
        }

        if (isAuth0UpdateRequired)
        {
            await identityProvider.UpdateUserProfileAsync(
                user.IdentityId,
                updateUserProfileDto,
                cancellationToken);
        }

        await userRepository.UpdateAsync(user, cancellationToken);

        return mapper.Map<GetUserDto>(user);
    }

    public async Task DeactivateUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);
        await identityProvider.BlockUserAsync(user.IdentityId, cancellationToken);
    }

    public async Task ActivateUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);
        await identityProvider.UnblockUserAsync(user.IdentityId, cancellationToken);
    }

    public async Task DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);
        await identityProvider.DeleteUserAsync(user.IdentityId, cancellationToken);
        await userRepository.DeleteAsync(user, cancellationToken);
    }

    public async Task<GetUserDto> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new UserNotFoundException(id.ToString(), IdentitySources.Internal);

        return mapper.Map<GetUserDto>(user);
    }

    public async Task<GetUserDto?> GetUserByIdentityIdAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdentityIdAsync(identityId, cancellationToken);
        return user is null ? null : mapper.Map<GetUserDto>(user);
    }

    public async Task<PagedResponse<GetUserDto>> GetUsersAsync(
        UserFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var userFilter = mapper.Map<UserFilter>(filter);
        var users = await userRepository.GetFilteredAsync(userFilter, cancellationToken);

        return mapper.Map<PagedResponse<GetUserDto>>(users);
    }

    private async Task<User> GetExistingUserAsync(
        Guid id,
        string identitySource,
        CancellationToken cancellationToken)
    {
        return await userRepository.GetByIdAsync(id, cancellationToken)
               ?? throw new UserNotFoundException(id.ToString(), identitySource);
    }
}
