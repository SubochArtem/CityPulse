using Users.Business.DTOs;

namespace Users.Business.Interfaces;

public interface IUserService
{
    public Task<GetUserDto> CreateUserAsync(
        CreateUserDto createUserDto,
        CancellationToken cancellationToken = default);

    public Task<GetUserDto> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    public Task<GetUserDto?> GetUserByIdentityIdAsync(
        string identityId,
        CancellationToken cancellationToken = default);

    public Task<IEnumerable<GetUserDto>> GetAllUsersAsync(
        CancellationToken cancellationToken = default);

    public Task DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    public Task ActivateUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    public Task DeactivateUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
