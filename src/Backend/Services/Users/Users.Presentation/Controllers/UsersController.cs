using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Business.DTOs;
using Users.Business.Interfaces;
using Users.Presentation.Authorization;

namespace Users.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.ReadUser)]
    public async Task<GetUserDto> GetUserById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _userService.GetUserByIdAsync(id, cancellationToken);
    }

    [HttpGet]
    [Authorize(Policy = Policies.ReadUser)]
    public async Task<IEnumerable<GetUserDto>> GetAllUsers(
        CancellationToken cancellationToken = default)
    {
        return await _userService.GetAllUsersAsync(cancellationToken);
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Policies.DeactivateUser)]
    public async Task DeactivateUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _userService.DeactivateUserAsync(id, cancellationToken);
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = Policies.ActivateUser)]
    public async Task ActivateUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _userService.ActivateUserAsync(id, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.DeleteUser)]
    public async Task DeleteUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _userService.DeleteUserAsync(id, cancellationToken);
    }
}
