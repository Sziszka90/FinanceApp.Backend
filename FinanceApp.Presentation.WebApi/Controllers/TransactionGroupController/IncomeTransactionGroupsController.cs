using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
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
public class IncomeTransactionGroupsController : ControllerBase
{
  #region Members

  private readonly IMediator _mediator;

  #endregion

  #region Constructors

  public IncomeTransactionGroupsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  #endregion

  #region Methods

  [HttpGet]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetIncomeTransactionGroupDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetIncomeTransactionGroupDto>>> GetTransactionGroups()
  {
    var result = await _mediator.Send(new GetAllIncomeGroupsQuery());
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionGroupDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetIncomeTransactionGroupDto>> GetTransactionGroups([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetIncomeGroupByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetIncomeTransactionGroupDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetIncomeTransactionGroupDto>> CreateTransactionGroup([FromBody] CreateIncomeTransactionGroupDto createIncomeTransactionGroupDto)
  {
    var result = await _mediator.Send(new CreateIncomeGroupCommand(createIncomeTransactionGroupDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut]
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

  [HttpDelete("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteTransaction([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteIncomeGroupCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }

  #endregion
}