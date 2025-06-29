using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.User.UserCommands;
using FinanceApp.Application.User.UserQueries;
using FinanceApp.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanceApp.Presentation.WebApi.Controllers.UsersController;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsersController : ControllerBase
{
  private readonly IMediator _mediator;

  public UsersController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet("{id}/confirm-email")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ConfirmEmail([FromRoute] Guid id, [FromQuery] string token)
  {
    var result = await _mediator.Send(new ConfirmUserEmailCommand(id, token));
    return this.RedirectToUrl(result, "https://financeapp.fun/login");
  }

  [HttpPost("forgot-password")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ForgotPassword([FromBody] EmailDto email)
  {
    var result = await _mediator.Send(new ForgotPasswordCommand(email));
    return this.GetResult(result);
  }

  [HttpPost("update-password")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto updatePasswordDto)
  {
    var result = await _mediator.Send(new UpdatePasswordCommand(updatePasswordDto));
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Authorize]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetUserDto>> GetUser([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetUserByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpGet]
  [Authorize]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetUserDto>> GetActiveUser()
  {
    var result = await _mediator.Send(new GetActiveUserQuery());
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetUserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
  {
    var result = await _mediator.Send(new CreateUserCommand(createUserDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut]
  [Authorize]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetSavingDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetUserDto>> UpdateSaving([FromBody] UpdateUserDto updateUserDto)
  {
    var result = await _mediator.Send(new UpdateUserCommand(updateUserDto));
    return this.GetResult(result);
  }

  [HttpDelete("{id}")]
  [Authorize]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteUser([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteUserCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }
}
