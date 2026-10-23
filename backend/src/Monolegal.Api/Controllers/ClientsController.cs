using Microsoft.AspNetCore.Mvc;
using Monolegal.Application.Dtos;
using Monolegal.Application.Services;

namespace Monolegal.Api.Controllers;

[ApiController]
[Route("api/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientQueryService _clientQueryService;

    public ClientsController(IClientQueryService clientQueryService)
    {
        _clientQueryService = clientQueryService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClientDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _clientQueryService.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    [ProducesResponseType<ClientDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var client = await _clientQueryService.GetByIdAsync(id, cancellationToken);

        return client is null ? NotFound() : Ok(client);
    }
}
