using Asp.Versioning;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.ValidateToken;
using FinanceApp.Backend.Application.Dtos.TokenDtos;
using FinanceApp.Backend.Application.UserApi.UserCommands.ValidateToken;
using FinanceApp.Backend.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
  public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ValidateTokenCommand(request.Token, cancellationToken));
    return this.GetResult(result);
  }
}
