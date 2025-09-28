using Asp.Versioning;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.Login;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.Logout;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.ResetToken;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.SetToken;
using FinanceApp.Backend.Application.AuthApi.AuthQueries.CheckQuery;
using FinanceApp.Backend.Application.Dtos.AuthDtos;
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

  [HttpPost("logout")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<LoginResponseDto>> Logout(CancellationToken cancellationToken)
  {
    var refreshToken = Request.Cookies["RefreshToken"];
    var token = Request.Cookies["Token"];

    var result = await _mediator.Send(new LogoutCommand(token, refreshToken, cancellationToken));

    if (!result.IsSuccess)
    {
      return this.GetResult(result);
    }

    result = await _mediator.Send(new ResetTokenCommand(cancellationToken));

    return this.GetResult(result);
  }

  [HttpGet("check")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<LoginResponseDto>> Check(CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new CheckQuery(cancellationToken));

    return this.GetResult(result);
  }
}
