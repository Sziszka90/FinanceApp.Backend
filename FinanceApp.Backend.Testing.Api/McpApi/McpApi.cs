using System.Net;
using System.Net.Http.Json;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FinanceApp.Backend.Testing.Api.McpControllerTests;

public class McpApi : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly HttpClient _client;

  public McpApi(WebApplicationFactory<Program> factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task Post_ValidMcpRequest_ReturnsSuccessEnvelope()
  {
    var mcpRequest = new McpRequest
    {
      Action = "get_top_transaction_groups",
      Parameters = new Dictionary<string, object>
      {
        { "start_date", "2023-01-01T00:00:00Z" },
        { "end_date", "2023-01-31T23:59:59Z" },
        { "top", 5 },
        { "user_id", Guid.NewGuid().ToString() },
        { "correlation_id", Guid.NewGuid().ToString() }
      }
    };

    var response = await _client.PostAsJsonAsync("/api/v1/mcp", mcpRequest);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var envelope = await response.Content.ReadFromJsonAsync<McpEnvelope>();
    Assert.NotNull(envelope);
    Assert.Equal("get_top_transaction_groups", envelope.ToolName);
    Assert.NotNull(envelope.Payload);
  }

  [Fact]
  public async Task Post_InvalidArgumentTypes_ReturnsBadRequest()
  {
    var mcpRequest = new McpRequest
    {
      Action = "get_top_transaction_groups",
      Parameters = new Dictionary<string, object>
      {
        { "start_date", 123 },
        { "end_date", "not-a-date" },
        { "top", "not-an-int" },
        { "user_id", Guid.NewGuid().ToString() },
        { "correlation_id", Guid.NewGuid().ToString() }
      }
    };

    var response = await _client.PostAsJsonAsync("/api/v1/mcp", mcpRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }
}
