using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceApp.Application.Converters;

public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
  // Define the custom format pattern (e.g., "2025-01-02 00:00:00+00")
  private static readonly string DateFormat = "yyyy-MM-dd HH:mm:sszzz";

  public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    // Read the string value
    var dateString = reader.GetString();

    // Parse the string into DateTimeOffset
    var result = DateTimeOffset.Parse(dateString!, null, DateTimeStyles.AssumeUniversal);

    return result;
  }

  public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
  {
    // Use the same custom format for serialization
    writer.WriteStringValue(value.ToString(DateFormat));
  }
}
