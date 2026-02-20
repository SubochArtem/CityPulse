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
    IValidator<CreateUserDto> createValidator) : IUserService
{
    private readonly IValidator<CreateUserDto> _createValidator = createValidator;
    private readonly IIdentityProvider _identityProvider = identityProvider;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<GetUserDto> CreateUserAsync(
        CreateUserDto createUserDto,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(
            createUserDto,
            cancellationToken);

        var existingUser = await _userRepository.GetByAuth0UserIdAsync(
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

    public async Task DeactivateUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);
        await _identityProvider.BlockUserAsync(user.Auth0UserId, cancellationToken);
    }

    public async Task ActivateUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);
        await _identityProvider.UnblockUserAsync(user.Auth0UserId, cancellationToken);
    }

    public async Task DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await GetExistingUserAsync(id, IdentitySources.Internal, cancellationToken);
        await _identityProvider.DeleteUserAsync(user.Auth0UserId, cancellationToken);
        await _userRepository.DeleteAsync(user, cancellationToken);
    }

    public async Task<GetUserDto?> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user?.Adapt<GetUserDto>();
    }

    public async Task<GetUserDto?> GetUserByAuth0IdAsync(
        string auth0UserId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByAuth0UserIdAsync(auth0UserId, cancellationToken);
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
