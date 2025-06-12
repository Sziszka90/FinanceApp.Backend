using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Transaction.TransactionCommands;
using FinanceApp.Application.Transaction.TransactionQueries;
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
  private readonly IMediator _mediator;

  public TransactionsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet("summary")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetTransactionDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetTransactionDto>>> GetTransactionsSummary()
  {
    var result = await _mediator.Send(new GetTransactionSumQuery());
    return this.GetResult(result);
  }

  [HttpGet]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetTransactionDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetTransactionDto>>> GetTransactions()
  {
    var result = await _mediator.Send(new GetAllTransactionQuery());
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionDto>> GetTransactions([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetTransactionByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionDto>> CreateTransaction([FromBody] CreateTransactionDto createTransactionDto)
  {
    var result = await _mediator.Send(new CreateTransactionCommand(createTransactionDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionDto>> UpdateTransaction([FromBody] UpdateTransactionDto updateTransactionDto)
  {
    var result = await _mediator.Send(new UpdateTransactionCommand(updateTransactionDto));
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
    var result = await _mediator.Send(new DeleteTransactionCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }
}
