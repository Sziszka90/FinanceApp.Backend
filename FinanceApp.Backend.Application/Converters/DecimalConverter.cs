using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceApp.Backend.Application.Converters;

public class DecimalConverter : JsonConverter<decimal>
{
  public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    if (reader.TokenType == JsonTokenType.String)
    {
      var value = reader.GetString();
      if (decimal.TryParse(value, CultureInfo.InvariantCulture, out var result))
      {
        return result;
      }

      throw new JsonException($"Unable to convert \"{value}\" to decimal.");
    }

    return reader.GetDecimal();
  }

  public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
  {
    writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
  }
}
