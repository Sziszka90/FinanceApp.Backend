using Asp.Versioning;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.Login;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.Logout;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.Refresh;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.SetToken;
using FinanceApp.Backend.Application.Dtos.AuthDtos;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Backend.Presentation.WebApi.Controllers.AuthController;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
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
  [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginRequestDto, CancellationToken cancellationToken)
  {
    var loginResult = await _mediator.Send(new LoginCommand(loginRequestDto, cancellationToken));

    if (loginResult.IsSuccess)
    {
      var setTokenResult = await _mediator.Send(new SetTokenCommand(loginResult.Data!.Token, loginResult.Data.RefreshToken, cancellationToken));

      return this.GetResult(setTokenResult);
    }

    return this.GetResult(loginResult);
  }

  [HttpPost("refresh")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> Refresh(CancellationToken cancellationToken)
  {
    var refreshToken = Request.Cookies["RefreshToken"];

    if (string.IsNullOrEmpty(refreshToken))
    {
      return BadRequest("Refresh token not found.");
    }

    var result = await _mediator.Send(new RefreshCommand(refreshToken, cancellationToken));

    if (result.IsSuccess)
    {
      var setTokenResult = await _mediator.Send(new SetTokenCommand(result.Data!, refreshToken, cancellationToken));
      return this.GetResult(setTokenResult);
    }
    return this.GetResult(result);
  }

  [HttpPost("logout")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<LoginResponseDto>> Logout(CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new LogoutCommand(cancellationToken));
    return this.GetResult(result);
  }
}
