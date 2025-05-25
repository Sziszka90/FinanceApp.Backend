using FinanceApp.Application.Auth;
using FinanceApp.Application.Dtos.AuthDtos;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanceApp.Presentation.WebApi.Controllers.AuthController;

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
  public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginRequestDto)
  {
    var result = await _mediator.Send(new LoginCommand(loginRequestDto));
    return this.GetResult(result);
  }
}
