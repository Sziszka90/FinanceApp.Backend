using System.Security.Claims;
using Asp.Versioning;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.CreateTransaction;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.DeleteTransaction;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.ImportPdf;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UpdateTransaction;
using FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetAllTransaction;
using FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionById;
using FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionCursorPage;
using FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionSum;
using FinanceApp.Backend.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Backend.Presentation.WebApi.Controllers.TransactionsController;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[ApiController]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly IPdfImportStatusStore _importStatuses;

  public TransactionsController(IMediator mediator, IPdfImportStatusStore importStatuses)
  {
    _mediator = mediator;
    _importStatuses = importStatuses;
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

  [HttpGet("page")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(TransactionCursorPageDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<TransactionCursorPageDto>> GetTransactionsPage(
    CancellationToken cancellationToken,
    [FromQuery] int pageSize = 50,
    [FromQuery] string? cursor = null,
    [FromQuery] TransactionFilter? transactionFilter = null)
  {
    var result = await _mediator.Send(new GetTransactionCursorPageQuery(pageSize, cursor, transactionFilter, cancellationToken));
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

  [HttpPost("import")]
  [Produces("application/json")]
  [Consumes("multipart/form-data")]
  [ProducesResponseType(StatusCodes.Status202Accepted)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ImportTransactions([FromForm] UploadPdfFileDto uploadPdfFileDto, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ImportPdfCommand(uploadPdfFileDto), cancellationToken);
    return this.GetResult(result, StatusCodes.Status202Accepted);
  }

  [HttpGet("import/{correlationId:guid}/status")]
  [ProducesResponseType(typeof(PdfImportStatusDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<PdfImportStatusDto>> GetPdfImportStatus(
    [FromRoute] Guid correlationId, CancellationToken cancellationToken)
  {
    var ownerEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(ownerEmail))
    {
      return Unauthorized();
    }

    var status = await _importStatuses.GetAsync(ownerEmail, correlationId, cancellationToken);
    return status is null ? NotFound() : Ok(status);
  }
}
