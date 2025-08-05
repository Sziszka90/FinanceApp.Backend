using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceApp.Backend.Application.Converters;

public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
  private static readonly string DateFormat = "yyyy-MM-dd HH:mm:sszzz";
  private static readonly string[] SupportedFormats = new[]
  {
    "yyyy-MM-dd HH:mm:sszzz",
    "yyyy-MM-dd HH:mm:ss.fffzzz",
    "yyyy-MM-ddTHH:mm:ssZ",
    "yyyy-MM-ddTHH:mm:ss.fffZ",
    "yyyy-MM-ddTHH:mm:sszzz",
    "yyyy-MM-ddTHH:mm:ss.fffzzz"
  };

  public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    var dateString = reader.GetString();

    if (string.IsNullOrEmpty(dateString))
      throw new FormatException("Date string cannot be null or empty.");

    // Try parsing with specific formats first
    if (DateTimeOffset.TryParseExact(dateString, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactResult))
      return exactResult;

    // Fallback to general parsing
    if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var generalResult))
      return generalResult;

    throw new FormatException($"Unable to parse '{dateString}' as a valid DateTimeOffset.");
  }
  public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
  {
    writer.WriteStringValue(value.ToString(DateFormat));
  }
}
