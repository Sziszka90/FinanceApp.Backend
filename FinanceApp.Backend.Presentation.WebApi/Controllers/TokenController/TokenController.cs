using FinanceApp.Backend.Application.AuthApi.AuthCommands.ValidateToken;
using FinanceApp.Backend.Application.UserApi.UserCommands.ValidateToken;
using FinanceApp.Backend.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace FinanceApp.Backend.Presentation.WebApi.Controllers.TokenController;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Produces("application/json")]
public class TokenController : ControllerBase
{
  private readonly IMediator _mediator;

  public TokenController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost("validate")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(ValidateTokenResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ValidateToken([FromQuery] string token, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ValidateTokenCommand(token, cancellationToken));
    return this.GetResult(result);
  }
}
