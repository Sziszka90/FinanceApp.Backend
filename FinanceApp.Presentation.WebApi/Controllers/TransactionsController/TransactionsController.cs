using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.TransactionApi.TransactionCommands.CreateTransaction;
using FinanceApp.Application.TransactionApi.TransactionCommands.DeleteTransaction;
using FinanceApp.Application.TransactionApi.TransactionCommands.UpdateTransaction;
using FinanceApp.Application.TransactionApi.TransactionCommands.UploadCsv;
using FinanceApp.Application.TransactionApi.TransactionQueries.GetAllTransaction;
using FinanceApp.Application.TransactionApi.TransactionQueries.GetTransactionById;
using FinanceApp.Application.TransactionApi.TransactionQueries.GetTransactionSum;
using FinanceApp.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
  public async Task<ActionResult<List<GetTransactionDto>>> GetTransactionsSummary(CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new GetTransactionSumQuery(cancellationToken));
    return this.GetResult(result);
  }

  [HttpGet]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetTransactionDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetTransactionDto>>> GetTransactions(CancellationToken cancellationToken, TransactionFilter? transactionFilter = null)
  {
    var result = await _mediator.Send(new GetAllTransactionQuery(cancellationToken, transactionFilter));
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionDto>> GetTransactions([FromRoute] Guid id, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new GetTransactionByIdQuery(id, cancellationToken));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionDto>> CreateTransaction([FromBody] CreateTransactionDto createTransactionDto, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new CreateTransactionCommand(createTransactionDto, cancellationToken));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionDto>> UpdateTransaction([FromRoute] Guid id, [FromBody] UpdateTransactionDto updateTransactionDto, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new UpdateTransactionCommand(id, updateTransactionDto, cancellationToken));
    return this.GetResult(result);
  }

  [HttpDelete("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteTransaction([FromRoute] Guid id, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new DeleteTransactionCommand(id, cancellationToken));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }

  [HttpPost("upload-csv")]
  [Produces("application/json")]
  [Consumes("multipart/form-data")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> UploadCsv([FromForm] UploadCsvFileDto uploadCsvFileDto, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new UploadCsvCommand(uploadCsvFileDto, cancellationToken));
    return this.GetResult(result);
  }
}
