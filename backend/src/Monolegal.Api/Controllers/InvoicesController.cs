using Microsoft.AspNetCore.Mvc;
using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Application.Dtos;
using Monolegal.Application.Services;
using Monolegal.Domain.Entities;

namespace Monolegal.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController : ControllerBase
{
    private readonly IInvoiceQueryService _invoiceQueryService;
    private readonly IEmailPreviewService _emailPreviewService;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;

    public InvoicesController(
        IInvoiceQueryService invoiceQueryService,
        IEmailPreviewService emailPreviewService,
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository)
    {
        _invoiceQueryService = invoiceQueryService;
        _emailPreviewService = emailPreviewService;
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InvoiceDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> GetAll(
        [FromQuery] string? clientId,
        CancellationToken cancellationToken) =>
        Ok(await _invoiceQueryService.GetAllAsync(clientId, cancellationToken));

    [HttpGet("summary")]
    [ProducesResponseType<InvoiceSummaryDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InvoiceSummaryDto>> GetSummary(
        [FromQuery] string? clientId,
        CancellationToken cancellationToken) =>
        Ok(await _invoiceQueryService.GetSummaryAsync(clientId, cancellationToken));

    [HttpGet("{id}/email-preview")]
    [ProducesResponseType<EmailPreviewDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailPreviewDto>> GetEmailPreview(
        string id,
        CancellationToken cancellationToken)
    {
        var preview = await _emailPreviewService.GetByInvoiceIdAsync(id, cancellationToken);

        return preview is null ? NotFound() : Ok(preview);
    }

    [HttpPost]
    [ProducesResponseType<InvoiceDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<InvoiceDto>> Create(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId) || string.IsNullOrWhiteSpace(request.Number))
        {
            return BadRequest("El cliente y el número de factura son obligatorios.");
        }

        if (await _invoiceRepository.InvoiceNumberExistsAsync(request.Number.Trim(), cancellationToken))
        {
            return Conflict("Ya existe una factura con ese número.");
        }

        var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
        {
            return BadRequest("El cliente seleccionado no existe.");
        }

        var invoice = new Invoice
        {
            Id = $"invoice-{Guid.NewGuid():N}",
            ClientId = client.Id,
            Number = request.Number.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Amount = request.Amount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "COP" : request.Currency.Trim().ToUpperInvariant(),
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            Status = request.Status
        };

        var createdInvoice = await _invoiceRepository.CreateAsync(invoice, cancellationToken);

        var dto = new InvoiceDto(
            createdInvoice.Id,
            createdInvoice.ClientId,
            client.Name,
            createdInvoice.Number,
            createdInvoice.Description,
            createdInvoice.Amount,
            createdInvoice.Currency,
            createdInvoice.IssueDate,
            createdInvoice.DueDate,
            createdInvoice.Status.ToDbValue(),
            createdInvoice.Status.ToDisplayName(),
            createdInvoice.Status.IsReminderPending());

        return CreatedAtAction(nameof(GetEmailPreview), new { id = createdInvoice.Id }, dto);
    }
}
