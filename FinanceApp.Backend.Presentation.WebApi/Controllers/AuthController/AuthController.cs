using FinanceApp.Backend.Application.AuthApi.AuthCommands.Login;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.ValidateToken;
using FinanceApp.Backend.Application.Dtos.AuthDtos;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.UserApi.UserCommands.ValidateToken;
using FinanceApp.Backend.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Backend.Presentation.WebApi.Controllers.AuthController;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController : ControllerBase
{
  private readonly IMediator _mediator;
  public AuthController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost("login")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginRequestDto, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new LoginCommand(loginRequestDto, cancellationToken));
    return this.GetResult(result);
  }

  [HttpPost("validate-token")]
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
