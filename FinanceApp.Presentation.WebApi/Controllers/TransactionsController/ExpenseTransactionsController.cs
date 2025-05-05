using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;
using FinanceApp.Application.ExpenseTransaction.ExpenseTransactionCommands;
using FinanceApp.Application.ExpenseTransaction.ExpenseTransactionQueries;
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
public class ExpenseTransactionsController : ControllerBase
{
  #region Members

  private readonly IMediator _mediator;

  #endregion

  #region Constructors

  public ExpenseTransactionsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  #endregion

  #region Methods

  [HttpGet]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetExpenseTransactionDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetExpenseTransactionDto>>> GetTransactions()
  {
    var result = await _mediator.Send(new GetAllExpensesQuery());
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetExpenseTransactionDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetExpenseTransactionDto>> GetTransactions([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetExpenseByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetExpenseTransactionDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetExpenseTransactionDto>> CreateTransaction([FromBody] CreateExpenseTransactionDto createExpenseTransactionDto)
  {
    var result = await _mediator.Send(new CreateExpenseCommand(createExpenseTransactionDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut]
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

  [HttpDelete("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteTransaction([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteExpenseCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }

  #endregion
}