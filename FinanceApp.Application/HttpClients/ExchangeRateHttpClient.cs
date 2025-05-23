using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceApp.Application.Abstraction.HttpClients;
using FinanceApp.Application.Converters;
using FinanceApp.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Options;

namespace FinanceApp.Application.HttpClients;

public class ExchangeRateHttpClient : IExchangeRateHttpClient
{
  #region Members

  private readonly HttpClient _httpClient;
  private readonly ExchangeRateSettings _exchangeRateSettings;

  #endregion

  #region Constructors

  public ExchangeRateHttpClient(HttpClient httpClient, IOptions<ExchangeRateSettings> exchangeRateSetOptions)
  {
    _httpClient = httpClient;
    _exchangeRateSettings = exchangeRateSetOptions.Value;
  }

  #endregion

  #region Methods

  public async Task<ExchangeRateResponseDto?> GetDataAsync(string fromCurrency, string toCurrency)
  {
    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    options.Converters.Add(new DateTimeOffsetConverter());
    options.Converters.Add(new DecimalConverter());
    options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

    var endpoint = _exchangeRateSettings.Endpoint.Replace("API_KEY", _exchangeRateSettings.ApiKey);

    var response = await _httpClient.GetAsync(endpoint);
    var content = await response.Content.ReadAsStringAsync();
    var dto = JsonSerializer.Deserialize<ExchangeRateResponseDto>(content, options);
    return dto;
  }

  private HttpContent ConvertObjectToHttpContent(ExchangeRateResponseDto obj)
  {
    // Serialize object to JSON string
    var jsonString = JsonSerializer.Serialize(obj);

    // Create HttpContent using StringContent with the serialized JSON string
    return new StringContent(jsonString, Encoding.UTF8, "application/json");
  }

  #endregion
}