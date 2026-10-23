using Microsoft.AspNetCore.Mvc;
using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Application.Dtos;
using Monolegal.Application.Services;
using Monolegal.Domain.Entities;

namespace Monolegal.Api.Controllers;

[ApiController]
[Route("api/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientQueryService _clientQueryService;
    private readonly IClientRepository _clientRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public ClientsController(
        IClientQueryService clientQueryService,
        IClientRepository clientRepository,
        IInvoiceRepository invoiceRepository)
    {
        _clientQueryService = clientQueryService;
        _clientRepository = clientRepository;
        _invoiceRepository = invoiceRepository;
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

    [HttpPost]
    [ProducesResponseType<ClientDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ClientDto>> Create(
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("El nombre y el correo son obligatorios.");
        }

        var client = new Client
        {
            Id = $"client-{Guid.NewGuid():N}",
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Identification = request.Identification?.Trim() ?? string.Empty,
            IsActive = request.IsActive
        };

        var created = await _clientRepository.CreateAsync(client, cancellationToken);

        var dto = new ClientDto(created.Id, created.Name, created.Email, created.Identification, created.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
    }

    [HttpPost("with-invoice")]
    [ProducesResponseType<InvoiceDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<InvoiceDto>> CreateWithInvoice(
        [FromBody] CreateClientWithInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Client?.Name) || string.IsNullOrWhiteSpace(request.Client?.Email))
        {
            return BadRequest("El nombre y el correo del cliente son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(request.Invoice?.Number))
        {
            return BadRequest("El número de factura es obligatorio.");
        }

        if (await _invoiceRepository.InvoiceNumberExistsAsync(request.Invoice.Number.Trim(), cancellationToken))
        {
            return Conflict("Ya existe una factura con ese número.");
        }

        var client = new Client
        {
            Id = $"client-{Guid.NewGuid():N}",
            Name = request.Client.Name.Trim(),
            Email = request.Client.Email.Trim(),
            Identification = request.Client.Identification?.Trim() ?? string.Empty,
            IsActive = request.Client.IsActive
        };

        var createdClient = await _clientRepository.CreateAsync(client, cancellationToken);

        var invoice = new Invoice
        {
            Id = $"invoice-{Guid.NewGuid():N}",
            ClientId = createdClient.Id,
            Number = request.Invoice.Number.Trim(),
            Description = request.Invoice.Description?.Trim() ?? string.Empty,
            Amount = request.Invoice.Amount,
            Currency = string.IsNullOrWhiteSpace(request.Invoice.Currency) ? "COP" : request.Invoice.Currency.Trim().ToUpperInvariant(),
            IssueDate = request.Invoice.IssueDate,
            DueDate = request.Invoice.DueDate,
            Status = request.Invoice.Status
        };

        var createdInvoice = await _invoiceRepository.CreateAsync(invoice, cancellationToken);

        var dto = new InvoiceDto(
            createdInvoice.Id,
            createdInvoice.ClientId,
            createdClient.Name,
            createdInvoice.Number,
            createdInvoice.Description,
            createdInvoice.Amount,
            createdInvoice.Currency,
            createdInvoice.IssueDate,
            createdInvoice.DueDate,
            createdInvoice.Status.ToDbValue(),
            createdInvoice.Status.ToDisplayName(),
            createdInvoice.Status.IsReminderPending());

        return CreatedAtAction(
            actionName: "GetEmailPreview",
            controllerName: "Invoices",
            routeValues: new { id = createdInvoice.Id },
            value: dto);
    }
}
