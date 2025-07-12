using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceApp.Application.Converters;

public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
  private static readonly string DateFormat = "yyyy-MM-dd HH:mm:sszzz";

  public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    var dateString = reader.GetString();

    var result = DateTimeOffset.Parse(dateString!, null, DateTimeStyles.AssumeUniversal);

    return result;
  }

  public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
  {
    writer.WriteStringValue(value.ToString(DateFormat));
  }
}
