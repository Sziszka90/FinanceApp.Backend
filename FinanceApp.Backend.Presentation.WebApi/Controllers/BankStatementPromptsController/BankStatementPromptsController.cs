using Asp.Versioning;
using FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptCommands;
using FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptQueries;
using FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;
using FinanceApp.Backend.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Backend.Presentation.WebApi.Controllers.BankStatementPromptsController;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[ApiController]
[Produces("application/json")]
public class BankStatementPromptsController : ControllerBase
{
  private readonly IMediator _mediator;

  public BankStatementPromptsController(IMediator mediator) => _mediator = mediator;

  [HttpGet]
  public async Task<ActionResult<List<BankStatementPromptDto>>> Get(CancellationToken cancellationToken)
    => this.GetResult(await _mediator.Send(new GetBankStatementPromptsQuery(), cancellationToken));

  [HttpPost]
  [Consumes("application/json")]
  public async Task<ActionResult<BankStatementPromptDto>> Create([FromBody] SaveBankStatementPromptDto prompt, CancellationToken cancellationToken)
    => this.GetResult(await _mediator.Send(new SaveBankStatementPromptCommand(prompt), cancellationToken), StatusCodes.Status201Created);

  [HttpPut("{id:guid}")]
  [Consumes("application/json")]
  public async Task<ActionResult<BankStatementPromptDto>> Update(Guid id, [FromBody] SaveBankStatementPromptDto prompt, CancellationToken cancellationToken)
    => this.GetResult(await _mediator.Send(new SaveBankStatementPromptCommand(prompt, id), cancellationToken));

  [HttpPut("{id:guid}/select")]
  public async Task<ActionResult<bool>> Select(Guid id, CancellationToken cancellationToken)
    => this.GetResult(await _mediator.Send(new SelectBankStatementPromptCommand(id), cancellationToken));

  [HttpDelete("selected")]
  public async Task<ActionResult<bool>> ClearSelection(CancellationToken cancellationToken)
    => this.GetResult(await _mediator.Send(new SelectBankStatementPromptCommand(null), cancellationToken));

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult<bool>> Delete(Guid id, CancellationToken cancellationToken)
    => this.GetResult(await _mediator.Send(new DeleteBankStatementPromptCommand(id), cancellationToken), StatusCodes.Status204NoContent);
}
