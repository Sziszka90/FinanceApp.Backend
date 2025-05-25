using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Application.Investment.InvestmentCommands;
using FinanceApp.Application.Investment.InvestmentQueries;
using FinanceApp.Presentation.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanceApp.Presentation.WebApi.Controllers.TransactionController;

[Route("api/[controller]")]
[Authorize]
[ApiController]
[Produces("application/json")]
public class InvestmentsController : ControllerBase
{
  private readonly IMediator _mediator;

  public InvestmentsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetInvestmentDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetSavingDto>>> GetInvestments()
  {
    var result = await _mediator.Send(new GetAllInvestmentsQuery());
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetInvestmentDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetInvestmentDto>> GetInvestments([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetInvestmentByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetInvestmentDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetInvestmentDto>> CreateInvestment([FromBody] CreateInvestmentDto createInvestmentDto)
  {
    var result = await _mediator.Send(new CreateInvestmentCommand(createInvestmentDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetInvestmentDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetInvestmentDto>> UpdateInvestment([FromBody] UpdateInvestmentDto updateInvestmentDto)
  {
    var result = await _mediator.Send(new UpdateInvestmentCommand(updateInvestmentDto));
    return this.GetResult(result);
  }

  [HttpDelete("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteInvestment([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteInvestmentCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }
}
