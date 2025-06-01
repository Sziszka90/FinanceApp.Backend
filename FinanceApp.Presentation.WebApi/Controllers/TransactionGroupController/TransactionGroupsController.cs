using System.Reflection;
using System.Text.Json;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.TransactionGroup.TransactionGroupCommands;
using FinanceApp.Application.TransactionGroup.TransactionGroupQueries;
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
public class TransactionGroupsController : ControllerBase
{
  private readonly IMediator _mediator;

  public TransactionGroupsController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(List<GetTransactionGroupDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<List<GetTransactionGroupDto>>> GetTransactionGroups()
  {
    var result = await _mediator.Send(new GetAllTransactionGroupsQuery());
    return this.GetResult(result);
  }

  [HttpGet("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionGroupDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionGroupDto>> GetTransactionGroup([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new GetTransactionGroupByIdQuery(id));
    return this.GetResult(result);
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("multipart/form-data")]
  [ProducesResponseType(typeof(GetTransactionGroupDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionGroupDto>> CreateTransactionGroup([FromForm] IFormFile? image, [FromForm] CreateTransactionGroupDto createTransactionGroupDto)
  {
    var result = await _mediator.Send(new CreateTransactionGroupCommand(createTransactionGroupDto, image));
    return this.GetResult(result, StatusCodes.Status201Created);
  }

  [HttpPut]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(GetTransactionGroupDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<GetTransactionGroupDto>> UpdateTransactionGroup([FromForm] IFormFile? image, [FromForm] UpdateTransactionGroupDto updateTransactionGroupDto)
  {
    var result = await _mediator.Send(new UpdateTransactionGroupCommand(updateTransactionGroupDto, image));
    return this.GetResult(result);
  }

  [HttpGet("{id}/image")]
  [Produces("image/jpeg", "image/png", "image/gif")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetImage(Guid id)
  {
    var result = await _mediator.Send(new GetTransactionGroupImageQuery(id));

    if (result.IsSuccess && result.Data is not null) return this.GetResult(result, StatusCodes.Status404NotFound);
    return File(result.Data!.Data, result.Data!.ContentType);
  }

  [HttpDelete("{id}")]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult> DeleteTransactionGroup([FromRoute] Guid id)
  {
    var result = await _mediator.Send(new DeleteTransactionGroupCommand(id));
    return this.GetResult(result, StatusCodes.Status204NoContent);
  }
}
