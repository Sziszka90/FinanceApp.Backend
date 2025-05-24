using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.ExpenseTransaction.ExpenseTransactionCommands;
using FinanceApp.Application.ExpenseTransaction.ExpenseTransactionQueries;
using FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;
using FinanceApp.Application.IncomeTransaction.IncomeTransactionQueries;
using FinanceApp.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanceApp.Presentation.WebApi.Controllers.TransactionsController;

[Route("api/[controller]")]
[Authorize]
[ApiController]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
  #region Members

  private readonly IMediator _mediator;

  #endregion

  #region Constructors

  public TransactionsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  #endregion

  #region Methods
  [HttpGet("expense/summary")]
  [AllowAnonymous]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetExpenseTransactionDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetExpenseTransactionDto>>> GetExpenseTransactionsSummary()
  {
    var result = await _mediator.Send(new GetExpenseSumQuery());
    return this.GetResult(result);
  }

  [HttpGet("income/summary")]
  [AllowAnonymous]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetIncomeTransactionDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetIncomeTransactionDto>>> GetIncomeTransactionsSummary()
  {
    var result = await _mediator.Send(new GetIncomeSumQuery());
    return this.GetResult(result);
  }

  [HttpGet("expense")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetExpenseTransactionDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetExpenseTransactionDto>>> GetExpenseTransactions()
  {
    var result = await _mediator.Send(new GetAllExpensesQuery());
    return this.GetResult(result);
  }

  [HttpGet("income")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetIncomeTransactionDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetIncomeTransactionDto>>> GetIncomeTransactions()
  {
    var result = await _mediator.Send(new GetAllIncomesQuery());
    return this.GetResult(result);
  }

  [HttpGet("expense/{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetExpenseTransactionDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetExpenseTransactionDto>> GetExpenseTransactions([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetExpenseByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpGet("income/{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetIncomeTransactionDto>> GetIncomeTransactions([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetIncomeByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpPost("expense")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetExpenseTransactionDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetExpenseTransactionDto>> CreateExpenseTransaction([FromBody] CreateExpenseTransactionDto createExpenseTransactionDto)
  {
    var result = await _mediator.Send(new CreateExpenseCommand(createExpenseTransactionDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPost("income")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetIncomeTransactionDto>> CreateIncomeTransaction([FromBody] CreateIncomeTransactionDto createIncomeTransactionDto)
  {
    var result = await _mediator.Send(new CreateIncomeCommand(createIncomeTransactionDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut("expense")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetExpenseTransactionDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetExpenseTransactionDto>> UpdateExpenseTransaction([FromBody] UpdateExpenseTransactionDto updateExpenseTransactionDto)
  {
    var result = await _mediator.Send(new UpdateExpenseCommand(updateExpenseTransactionDto));
    return this.GetResult(result);
  }

  [HttpPut("income")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetIncomeTransactionDto>> UpdateIncomeTransaction([FromBody] UpdateIncomeTransactionDto updateIncomeTransactionDto)
  {
    var result = await _mediator.Send(new UpdateIncomeCommand(updateIncomeTransactionDto));
    return this.GetResult(result);
  }

  [HttpDelete("expense/{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteExpenseTransaction([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteExpenseCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }

  [HttpDelete("income/{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteIncomeTransaction([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteIncomeCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }

  #endregion
}
