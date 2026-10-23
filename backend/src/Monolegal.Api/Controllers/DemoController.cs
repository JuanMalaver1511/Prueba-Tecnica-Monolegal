using Microsoft.AspNetCore.Mvc;
using Monolegal.Application.Abstractions.Seed;

namespace Monolegal.Api.Controllers;

[ApiController]
[Route("api/demo")]
public sealed class DemoController : ControllerBase
{
    private readonly IDemoDataSeeder _demoDataSeeder;

    public DemoController(IDemoDataSeeder demoDataSeeder)
    {
        _demoDataSeeder = demoDataSeeder;
    }

    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await _demoDataSeeder.ResetAsync(cancellationToken);

        return NoContent();
    }
}
