using Asp.Versioning;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.McpApi.McpCommands;
using FinanceApp.Backend.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Backend.Presentation.WebApi.Controllers.McpController;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Produces("application/json")]
public class McpController : ControllerBase
{
  private readonly IMediator _mediator;
  public McpController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(McpEnvelope), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<McpEnvelope>> CallMcpTool(McpRequest mcpRequest, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new McpCommand(mcpRequest, cancellationToken));
    return this.GetResult(result);
  }
}
