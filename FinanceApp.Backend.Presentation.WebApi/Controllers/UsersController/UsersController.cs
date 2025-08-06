using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.UserApi.UserCommands.ConfirmUserEmail;
using FinanceApp.Backend.Application.UserApi.UserCommands.CreateUser;
using FinanceApp.Backend.Application.UserApi.UserCommands.DeleteUser;
using FinanceApp.Backend.Application.UserApi.UserCommands.ForgotPassword;
using FinanceApp.Backend.Application.UserApi.UserCommands.ResendConfirmationEmail;
using FinanceApp.Backend.Application.UserApi.UserCommands.UpdatePassword;
using FinanceApp.Backend.Application.UserApi.UserCommands.UpdateUser;
using FinanceApp.Backend.Application.UserApi.UserQueries.GetActiveUser;
using FinanceApp.Backend.Application.UserApi.UserQueries.GetUserById;
using FinanceApp.Backend.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace FinanceApp.Backend.Presentation.WebApi.Controllers.UsersController;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
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

  [HttpPost("resend-confirmation-email")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ResendConfirmationEmail([FromBody] EmailDto email, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ResendConfirmationEmailCommand(email, cancellationToken));
    return this.GetResult(result);
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
  public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest updatePasswordDto, CancellationToken cancellationToken)
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
  public async Task<ActionResult<GetUserDto>> UpdateUser([FromBody] UpdateUserRequest updateUserDto, CancellationToken cancellationToken)
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
