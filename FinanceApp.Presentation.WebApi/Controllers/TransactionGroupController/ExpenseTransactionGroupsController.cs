using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupCommands;
using FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupQueries;
using FinanceApp.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanceApp.Presentation.WebApi.Controllers.TransactionGroupController;

[Route("api/[controller]")]
[Authorize]
[ApiController]
[Produces("application/json")]
public class ExpenseTransactionGroupsController : ControllerBase
{
  #region Members

  private readonly IMediator _mediator;

  #endregion

  #region Constructors

  public ExpenseTransactionGroupsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  #endregion

  #region Methods

  [HttpGet]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetExpenseTransactionGroupDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetExpenseTransactionGroupDto>>> GetTransactionGroups()
  {
    var result = await _mediator.Send(new GetAllExpenseGroupsQuery());
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetExpenseTransactionGroupDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetExpenseTransactionGroupDto>> GetTransactionGroups([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetExpenseGroupByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetExpenseTransactionGroupDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetExpenseTransactionGroupDto>> CreateTransactionGroup([FromBody] CreateExpenseTransactionGroupDto createExpenseTransactionGroupDto)
  {
    var result = await _mediator.Send(new CreateExpenseGroupCommand(createExpenseTransactionGroupDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetExpenseTransactionGroupDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetExpenseTransactionGroupDto>> UpdateExpenseTransactionGroup([FromBody] UpdateExpenseTransactionGroupDto updateExpenseTransactionGroupDto)
  {
    var result = await _mediator.Send(new UpdateExpenseGroupCommand(updateExpenseTransactionGroupDto));
    return this.GetResult(result);
  }

  [HttpDelete("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteTransactionGroup([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteExpenseGroupCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }

  #endregion
}