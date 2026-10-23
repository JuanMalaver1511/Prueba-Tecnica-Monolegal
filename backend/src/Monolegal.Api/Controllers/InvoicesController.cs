using Microsoft.AspNetCore.Mvc;
using Monolegal.Application.Dtos;
using Monolegal.Application.Services;

namespace Monolegal.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController : ControllerBase
{
    private readonly IInvoiceQueryService _invoiceQueryService;
    private readonly IEmailPreviewService _emailPreviewService;

    public InvoicesController(
        IInvoiceQueryService invoiceQueryService,
        IEmailPreviewService emailPreviewService)
    {
        _invoiceQueryService = invoiceQueryService;
        _emailPreviewService = emailPreviewService;
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
}
