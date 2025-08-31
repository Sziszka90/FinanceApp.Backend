using System.Net;
using System.Net.Http.Json;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Testing.Api.Base;

namespace FinanceApp.Backend.Testing.Api.McpApi;

public class McpApi : TestBase
{
  [Fact]
  public async Task Post_ValidMcpRequest_ReturnsSuccessEnvelope()
  {
    // arrange
    await InitializeAsync();
    var mcpRequest = new McpRequest
    {
      ToolName = "GetTopTransactionGroups",
      Parameters = new Dictionary<string, object>
      {
        { "start_date", "2023-01-01T00:00:00Z" },
        { "end_date", "2023-01-31T23:59:59Z" },
        { "top", 5 },
        { "user_id", Guid.NewGuid().ToString() },
        { "correlation_id", Guid.NewGuid().ToString() }
      }
    };

    // act
    var response = await Client.PostAsync(MCP, CreateContent(mcpRequest));
    var result = await GetContentAsync<McpEnvelope>(response);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    Assert.NotNull(result);
    Assert.Equal("get_top_transaction_groups", result.ToolName);
    Assert.NotNull(result.Payload);
  }

  [Fact]
  public async Task Post_InvalidArgumentTypes_ReturnsBadRequest()
  {
    var mcpRequest = new McpRequest
    {
      ToolName = "GetTopTransactionGroups",
      Parameters = new Dictionary<string, object>
      {
        { "start_date", 123 },
        { "end_date", "not-a-date" },
        { "top", "not-an-int" },
        { "user_id", Guid.NewGuid().ToString() },
        { "correlation_id", Guid.NewGuid().ToString() }
      }
    };

    var response = await Client.PostAsync(MCP, CreateContent(mcpRequest));

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }
}
