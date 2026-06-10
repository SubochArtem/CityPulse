using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Business.DTOs;
using Users.Business.Interfaces;
using Users.Business.Responses;
using Users.Presentation.Authorization;

namespace Users.Presentation.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.ReadUser)]
    public async Task<GetUserDto> GetUserById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await userService.GetUserByIdAsync(id, cancellationToken);
    }

    [HttpGet]
    [Authorize(Policy = Policies.ReadUser)]
    public async Task<PagedResponse<GetUserDto>> GetUsers(
        [FromQuery] UserFilterDto filter,
        CancellationToken cancellationToken)
    {
        return await userService.GetUsersAsync(filter, cancellationToken);
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Policies.DeactivateUser)]
    public async Task DeactivateUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        await userService.DeactivateUserAsync(id, cancellationToken);
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = Policies.ActivateUser)]
    public async Task ActivateUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        await userService.ActivateUserAsync(id, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.DeleteUser)]
    public async Task DeleteUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        await userService.DeleteUserAsync(id, cancellationToken);
    }
    
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = Policies.UpdateUser)] 
    public async Task<GetUserDto> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UpdateUserProfileDto request,
        CancellationToken cancellationToken)
    {
        return await userService.UpdateUserAsync(id, request, cancellationToken);
    }
}
