using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
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
public class IncomeTransactionsController : ControllerBase
{
  #region Members

  private readonly IMediator _mediator;

  #endregion

  #region Constructors

  public IncomeTransactionsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  #endregion

  #region Methods

  [HttpGet]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetIncomeTransactionDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetIncomeTransactionDto>>> GetTransactions()
  {
    var result = await _mediator.Send(new GetAllIncomesQuery());
    return this.GetResult(result);
  }

  [HttpGet("summary")]
  [AllowAnonymous]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetIncomeTransactionDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetIncomeTransactionDto>>> GetTransactionsSummary()
  {
    var result = await _mediator.Send(new GetIncomeSumQuery());
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetIncomeTransactionDto>> GetTransactions([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetIncomeByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetIncomeTransactionDto>> CreateTransaction([FromBody] CreateIncomeTransactionDto createIncomeTransactionDto)
  {
    var result = await _mediator.Send(new CreateIncomeCommand(createIncomeTransactionDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut]
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

  [HttpDelete("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionDto), StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteTransaction([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteIncomeCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }

  #endregion
}