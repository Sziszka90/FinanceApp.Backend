using Asp.Versioning;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.McpApi.McpCommands;
using FinanceApp.Backend.Presentation.WebApi.Controllers.Common;
using FinanceApp.Backend.Presentation.WebApi.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Backend.Presentation.WebApi.Controllers.McpController;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Produces("application/json")]
public class McpController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly IValidator<McpRequest> _validator;
  public McpController(IMediator mediator, IValidator<McpRequest> validator)
  {
    _mediator = mediator;
    _validator = validator;
  }

  [HttpPost]
  [Produces("application/json")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(McpEnvelope), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<McpEnvelope>> CallMcpTool(CancellationToken cancellationToken)
  {
    using var reader = new StreamReader(Request.Body);
    var body = await reader.ReadToEndAsync(cancellationToken);

    var settings = new Newtonsoft.Json.JsonSerializerSettings
    {
      ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
      {
        NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy()
      }
    };

    var mcpRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<McpRequest>(body, settings);

    if (mcpRequest == null)
    {
      return BadRequest("Invalid payload");
    }

    mcpRequest.Parameters = mcpRequest.Parameters?.KeysToPascalCase() ?? new Dictionary<string, object>();

    var validationResult = await _validator.ValidateAsync(mcpRequest, cancellationToken);
    if (!validationResult.IsValid)
    {
      return BadRequest(validationResult.Errors);
    }

    var result = await _mediator.Send(new McpCommand(mcpRequest, cancellationToken));
    return this.GetResult(result);
  }
}
