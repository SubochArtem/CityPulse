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
    public async Task<ActionResult<GetUserDto>> GetUserById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        return Ok(user);
    }

    [HttpGet]
    [Authorize(Policy = Policies.ReadUser)]
    public async Task<ActionResult<IEnumerable<GetUserDto>>> GetAllUsers(
        CancellationToken cancellationToken = default)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Policies.DeactivateUser)]
    public async Task<IActionResult> DeactivateUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _userService.DeactivateUserAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = Policies.ActivateUser)]
    public async Task<IActionResult> ActivateUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _userService.ActivateUserAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.DeleteUser)]
    public async Task<IActionResult> DeleteUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _userService.DeleteUserAsync(id, cancellationToken);
        return NoContent();
    }
}
