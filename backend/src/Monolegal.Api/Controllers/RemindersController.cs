using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Monolegal.Api.Contracts;
using Monolegal.Application.Dtos;
using Monolegal.Application.Services;

namespace Monolegal.Api.Controllers;

[ApiController]
[Route("api/reminders")]
public sealed class RemindersController : ControllerBase
{
    private readonly IReminderProcessingService _reminderProcessingService;

    public RemindersController(IReminderProcessingService reminderProcessingService)
    {
        _reminderProcessingService = reminderProcessingService;
    }

    [HttpPost("process")]
    [ProducesResponseType<ReminderProcessingResult>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ReminderProcessingResult>> Process(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ProcessRemindersRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _reminderProcessingService.ProcessAsync(request?.ClientIds, cancellationToken);

        return Ok(result);
    }
}
