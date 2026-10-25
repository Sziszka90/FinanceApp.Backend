using Asp.Versioning;
using FinanceApp.Backend.Application.CategorizationRuleApi;
using FinanceApp.Backend.Application.Dtos.CategorizationRuleDtos;
using FinanceApp.Backend.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Backend.Presentation.WebApi.Controllers.CategorizationRulesController;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[ApiController]
[Produces("application/json")]
public class CategorizationRulesController(IMediator mediator) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<List<CategorizationRuleDto>>> Get(CancellationToken cancellationToken)
    => this.GetResult(await mediator.Send(new CategorizationRulesQuery(), cancellationToken));

  [HttpPost]
  [Consumes("application/json")]
  public async Task<ActionResult<CategorizationRuleDto>> Create([FromBody] SaveCategorizationRuleDto rule, CancellationToken cancellationToken)
    => this.GetResult(await mediator.Send(new SaveCategorizationRuleCommand(rule), cancellationToken), StatusCodes.Status201Created);

  [HttpPut("{id:guid}")]
  [Consumes("application/json")]
  public async Task<ActionResult<CategorizationRuleDto>> Update(Guid id, [FromBody] SaveCategorizationRuleDto rule, CancellationToken cancellationToken)
    => this.GetResult(await mediator.Send(new SaveCategorizationRuleCommand(rule, id), cancellationToken));

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult<bool>> Delete(Guid id, CancellationToken cancellationToken)
    => this.GetResult(await mediator.Send(new DeleteCategorizationRuleCommand(id), cancellationToken), StatusCodes.Status204NoContent);
}
