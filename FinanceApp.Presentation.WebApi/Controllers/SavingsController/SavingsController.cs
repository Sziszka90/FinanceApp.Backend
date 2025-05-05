using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.SavingDtos;
using FinanceApp.Application.Saving.SavingCommands;
using FinanceApp.Application.Saving.SavingQueries;
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
public class SavingsController : ControllerBase
{
  #region Members

  private readonly IMediator _mediator;

  #endregion

  #region Constructors

  public SavingsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  #endregion

  #region Methods

  [HttpGet]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetSavingDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetSavingDto>>> GetSavings()
  {
    var result = await _mediator.Send(new GetAllSavingsQuery());
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetSavingDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetSavingDto>> GetSavings([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetSavingByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetSavingDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetSavingDto>> CreateSaving([FromBody] CreateSavingDto createSavingDto)
  {
    var result = await _mediator.Send(new CreateSavingCommand(createSavingDto));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetSavingDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetSavingDto>> UpdateSaving([FromBody] UpdateSavingDto updateSavingDto)
  {
    var result = await _mediator.Send(new UpdateSavingCommand(updateSavingDto));
    return this.GetResult(result);
  }

  [HttpDelete("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteSaving([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteSavingCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }

  #endregion
}