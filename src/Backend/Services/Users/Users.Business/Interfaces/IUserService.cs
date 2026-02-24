using Users.Business.DTOs;

namespace Users.Business.Interfaces;

public interface IUserService
{
    Task<GetUserDto> CreateUserAsync(
        CreateUserDto createUserDto,
        CancellationToken cancellationToken = default);

    Task<GetUserDto?> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<GetUserDto?> GetUserByAuth0IdAsync(
        string auth0UserId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<GetUserDto>> GetAllUsersAsync(
        CancellationToken cancellationToken = default);

    Task DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task ActivateUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task DeactivateUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
