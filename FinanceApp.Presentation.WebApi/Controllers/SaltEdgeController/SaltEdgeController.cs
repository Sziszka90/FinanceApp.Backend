using FinanceApp.Application.Dtos.SaltEdgeDtos;
using FinanceApp.Application.SaltEdge.SaltEdgeCommands;
using FinanceApp.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Presentation.WebApi.Controllers.SaltEdgeController;

[ApiController]
[Route("api/saltedge/notify/676b108d-8af0-4e30-b10d-313edec1c1bf")]
public class SaltEdgeWebhookController : ControllerBase
{
  private readonly IMediator _mediator;

  public SaltEdgeWebhookController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> CreateSaltEdgeNotify([FromBody] CreateSaltEdgeNotifyRequestDto createSaltEdgeNotifyDto)
  {
    var result = await _mediator.Send(new CreateSaltEdgeNotifyCommand(createSaltEdgeNotifyDto));
    return this.GetResult(result, StatusCodes.Status200OK);
  }
}
