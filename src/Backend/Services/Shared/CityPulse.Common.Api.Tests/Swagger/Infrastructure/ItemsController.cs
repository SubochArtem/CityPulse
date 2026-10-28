using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CityPulse.Common.Api.Tests.Swagger.Infrastructure;

public sealed record ItemDto(Guid Id, string Name);

public sealed record CreateItemRequest(string Name);

public sealed record StatusRequest(int NewStatus);

public sealed class Box<T>
{
    public T? Value { get; init; }
}

public readonly struct Nothing;

[ApiController]
[Route("items")]
public sealed class ItemsController : ControllerBase
{
    [HttpGet("{id:guid}")]
    [Authorize]
    public Task<Box<ItemDto>> GetItem(Guid id) => throw new NotImplementedException();

    [HttpGet]
    [AllowAnonymous]
    public Task<Box<List<ItemDto>>> ListItems() => throw new NotImplementedException();

    [HttpPost]
    [Authorize]
    public Task<Box<ItemDto>> CreateItem([FromBody] CreateItemRequest request) => throw new NotImplementedException();

    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public Task<Box<ItemDto>> UpdateItem(Guid id, [FromBody] CreateItemRequest request) =>
        throw new NotImplementedException();

    [HttpPatch("{id:guid}/status")]
    [Authorize]
    public Task<Box<Nothing>> ChangeStatus(Guid id, [FromBody] StatusRequest request) =>
        throw new NotImplementedException();

    [HttpDelete("{id:guid}")]
    [Authorize]
    public Task<NoContentResult> DeleteItem(Guid id) => throw new NotImplementedException();

    [HttpGet("plain/{id:guid}")]
    [AllowAnonymous]
    public Task<ItemDto> GetPlainItem(Guid id) => throw new NotImplementedException();

    [HttpGet("wrapped-action-result")]
    [AllowAnonymous]
    public ActionResult<Box<ItemDto>> GetWrappedActionResult() => throw new NotImplementedException();
}
