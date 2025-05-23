using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupCommands;
using FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupQueries;
using FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupCommands;
using FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupQueries;
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
public class TransactionGroupsController : ControllerBase
{
  #region Members

  private readonly IMediator _mediator;

  #endregion

  #region Constructors

  public TransactionGroupsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  #endregion

  #region Methods

  [HttpGet("expense")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetExpenseTransactionGroupDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetExpenseTransactionGroupDto>>> GetExpenseTransactionGroups()
  {
    var result = await _mediator.Send(new GetAllExpenseGroupsQuery());
    return this.GetResult(result);
  }

  [HttpGet("income")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetIncomeTransactionGroupDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetIncomeTransactionGroupDto>>> GetIncomeTransactionGroups()
  {
    var result = await _mediator.Send(new GetAllIncomeGroupsQuery());
    return this.GetResult(result);
  }


  [HttpGet("expense/{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetExpenseTransactionGroupDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetExpenseTransactionGroupDto>> GetExpenseTransactionGroups([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetExpenseGroupByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpGet("income/{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionGroupDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetIncomeTransactionGroupDto>> GetIncomeTransactionGroups([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetIncomeGroupByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpPost("expense")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetExpenseTransactionGroupDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetExpenseTransactionGroupDto>> CreateExpenseTransactionGroup([FromBody] CreateExpenseTransactionGroupDto createExpenseTransactionGroupDto)
  {
    var result = await _mediator.Send(new CreateExpenseGroupCommand(createExpenseTransactionGroupDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPost("income")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionGroupDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetIncomeTransactionGroupDto>> CreateIncomeTransactionGroup([FromBody] CreateIncomeTransactionGroupDto createIncomeTransactionGroupDto)
  {
    var result = await _mediator.Send(new CreateIncomeGroupCommand(createIncomeTransactionGroupDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut("expense")]
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

  [HttpPut("income")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionGroupDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetIncomeTransactionGroupDto>> UpdateIncomeTransactionGroup([FromBody] UpdateIncomeTransactionGroupDto updateIncomeTransactionGroupDto)
  {
    var result = await _mediator.Send(new UpdateIncomeGroupCommand(updateIncomeTransactionGroupDto));
    return this.GetResult(result);
  }

  [HttpDelete("expense/{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteExpenseTransactionGroup([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteExpenseGroupCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }

  [HttpDelete("income/{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteIncomeTransactionGroup([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteIncomeGroupCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }

  #endregion
}