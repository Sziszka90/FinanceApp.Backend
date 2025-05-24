using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceApp.Application.Converters;

public class DecimalConverter : JsonConverter<decimal>
{
  public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    // If the value is a string, try to parse it as a decimal
    if (reader.TokenType == JsonTokenType.String)
    {
      var value = reader.GetString();
      if (decimal.TryParse(value, CultureInfo.InvariantCulture, out var result))
      {
        return result;
      }

      throw new JsonException($"Unable to convert \"{value}\" to decimal.");
    }

    // If the token is not a string, use the default behavior to read the decimal
    return reader.GetDecimal();
  }

  public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
  {
    writer.WriteStringValue(value.ToString());
  }
}
