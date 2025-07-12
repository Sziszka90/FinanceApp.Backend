using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.CreateTransactionGroup;
using FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.DeleteTransactionGroup;
using FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.UpdateTransactionGroup;
using FinanceApp.Application.TransactionGroupApi.TransactionGroupQueries.GetAllTransactionGroup;
using FinanceApp.Application.TransactionGroupApi.TransactionGroupQueries.GetTransactionGroupById;
using FinanceApp.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Presentation.WebApi.Controllers.TransactionGroupController;

[Route("api/[controller]")]
[Authorize]
[ApiController]
[Produces("application/json")]
public class TransactionGroupsController : ControllerBase
{
  private readonly IMediator _mediator;

  public TransactionGroupsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetTransactionGroupDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetTransactionGroupDto>>> GetTransactionGroups(CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new GetAllTransactionGroupQuery(cancellationToken));
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionGroupDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionGroupDto>> GetTransactionGroup([FromRoute] Guid id, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new GetTransactionGroupByIdQuery(id, cancellationToken));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionGroupDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionGroupDto>> CreateTransactionGroup([FromBody] CreateTransactionGroupDto createTransactionGroupDto, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new CreateTransactionGroupCommand(createTransactionGroupDto, cancellationToken));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionGroupDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionGroupDto>> UpdateTransactionGroup([FromRoute] Guid id, [FromBody] UpdateTransactionGroupDto updateTransactionGroupDto, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new UpdateTransactionGroupCommand(id, updateTransactionGroupDto, cancellationToken));
    return this.GetResult(result);
  }

  [HttpDelete("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteTransactionGroup([FromRoute] Guid id, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new DeleteTransactionGroupCommand(id, cancellationToken));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }
}
