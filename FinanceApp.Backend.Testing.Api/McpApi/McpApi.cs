using System.Net;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Testing.Api.Base;

namespace FinanceApp.Backend.Testing.Api.McpApi;

public class McpApi : TestBase
{
  [Fact]
  public async Task Post_ValidMcpRequest_ReturnsSuccessEnvelope()
  {
    // arrange
    await InitializeAsync();
    var mcpRequest = new Dictionary<string, object>
    {
      { "tool_name", "GetTopTransactionGroups" },
      { "parameters", new Dictionary<string, object>
        {
          { "start_date", "2023-01-01T00:00:00Z" },
          { "end_date", "2023-01-31T23:59:59Z" },
          { "top", 5 },
          { "user_id", CreatedUserId.ToString() },
          { "correlation_id", Guid.NewGuid().ToString() }
        }
      }
    };

    // act
    var response = await Client.PostAsync(MCP, CreateContent(mcpRequest));
    var result = await GetContentAsync<McpEnvelope>(response);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    Assert.NotNull(result);
    Assert.Equal(SupportedTools.GET_TOP_TRANSACTION_GROUPS, result.ToolName);
    Assert.NotNull(result.Payload);
  }

  [Fact]
  public async Task Post_InvalidArgumentTypes_ReturnsBadRequest()
  {
    var mcpRequest = new McpRequest
    {
      ToolName = SupportedTools.GET_TOP_TRANSACTION_GROUPS,
      Parameters = new Dictionary<string, object>
      {
        { "StartDate", 123 },
        { "EndDate", "not-a-date" },
        { "Top", "not-an-int" },
        { "UserId", Guid.NewGuid().ToString() },
        { "CorrelationId", Guid.NewGuid().ToString() }
      }
    };

    var response = await Client.PostAsync(MCP, CreateContent(mcpRequest));

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }
}
