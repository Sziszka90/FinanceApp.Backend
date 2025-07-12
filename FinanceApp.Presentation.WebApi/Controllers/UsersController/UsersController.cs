using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.UserApi.UserCommands.ConfirmUserEmail;
using FinanceApp.Application.UserApi.UserCommands.CreateUser;
using FinanceApp.Application.UserApi.UserCommands.DeleteUser;
using FinanceApp.Application.UserApi.UserCommands.ForgotPassword;
using FinanceApp.Application.UserApi.UserCommands.UpdatePassword;
using FinanceApp.Application.UserApi.UserCommands.UpdateUser;
using FinanceApp.Application.UserApi.UserQueries.GetActiveUser;
using FinanceApp.Application.UserApi.UserQueries.GetUserById;
using FinanceApp.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
  public async Task<IActionResult> ConfirmEmail([FromRoute] Guid id, [FromQuery] string token, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ConfirmUserEmailCommand(id, token, cancellationToken));
    return this.RedirectToUrl(result, "https://www.financeapp.fun/login");
  }

  [HttpPost("forgot-password")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ForgotPassword([FromBody] EmailDto email, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ForgotPasswordCommand(email, cancellationToken));
    return this.GetResult(result);
  }

  [HttpPost("update-password")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new UpdatePasswordCommand(updatePasswordDto, cancellationToken));
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Authorize]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetUserDto>> GetUser([FromRoute] Guid id, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new GetUserByIdQuery(id, cancellationToken));
    return this.GetResult(result);
  }

  [HttpGet]
  [Authorize]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetUserDto>> GetActiveUser(CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new GetActiveUserQuery(cancellationToken));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetUserDto>> CreateUser([FromBody] CreateUserDto createUserDto, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new CreateUserCommand(createUserDto, cancellationToken));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut]
  [Authorize]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetUserDto>> UpdateUser([FromBody] UpdateUserDto updateUserDto, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new UpdateUserCommand(updateUserDto, cancellationToken));
    return this.GetResult(result);
  }

  [HttpDelete("{id}")]
  [Authorize]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new DeleteUserCommand(id, cancellationToken));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }
}
