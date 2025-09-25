using System.Globalization;
using System.Text;
using System.Text.Json;
using FinanceApp.Backend.Application.Converters;

namespace FinanceApp.Backend.Testing.Unit.ConverterTests;

public class DateTimeOffsetConverterTests
{
  private readonly DateTimeOffsetConverter _converter;
  private readonly JsonSerializerOptions _options;

  public DateTimeOffsetConverterTests()
  {
    _converter = new DateTimeOffsetConverter();
    _options = new JsonSerializerOptions();
  }

  private static Utf8JsonReader CreateJsonReaderFromString(string json)
  {
    var bytes = Encoding.UTF8.GetBytes(json);
    var reader = new Utf8JsonReader(bytes);
    reader.Read(); // Move to the first token
    return reader;
  }

  public class ReadTests : DateTimeOffsetConverterTests
  {
    [Fact]
    public void Read_ValidDateTimeString_ShouldReturnCorrectDateTimeOffset()
    {
      // arrange
      var json = "\"2023-12-25 14:30:00+02:00\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(DateTimeOffset), _options);

      // assert
      Assert.Equal(new DateTimeOffset(2023, 12, 25, 14, 30, 0, TimeSpan.FromHours(2)), result);
    }

    [Fact]
    public void Read_UtcDateTimeString_ShouldReturnCorrectDateTimeOffset()
    {
      // arrange
      var json = "\"2023-12-25 14:30:00+00:00\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(DateTimeOffset), _options);

      // assert
      Assert.Equal(new DateTimeOffset(2023, 12, 25, 14, 30, 0, TimeSpan.Zero), result);
    }

    [Fact]
    public void Read_NegativeOffsetDateTimeString_ShouldReturnCorrectDateTimeOffset()
    {
      // arrange
      var json = "\"2023-12-25 14:30:00-05:00\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(DateTimeOffset), _options);

      // assert
      Assert.Equal(new DateTimeOffset(2023, 12, 25, 14, 30, 0, TimeSpan.FromHours(-5)), result);
    }

    [Fact]
    public void Read_Iso8601Format_ShouldParseCorrectly()
    {
      // arrange
      var json = "\"2023-12-25T14:30:00Z\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(DateTimeOffset), _options);

      // assert
      Assert.Equal(new DateTimeOffset(2023, 12, 25, 14, 30, 0, TimeSpan.Zero), result);
    }

    [Fact]
    public void Read_DateTimeWithMilliseconds_ShouldParseCorrectly()
    {
      // arrange
      var json = "\"2023-12-25 14:30:15.123+02:00\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(DateTimeOffset), _options);

      // assert
      Assert.Equal(new DateTimeOffset(2023, 12, 25, 14, 30, 15, 123, TimeSpan.FromHours(2)), result);
    }

    [Theory]
    [InlineData("\"2023-01-01 00:00:00+00:00\"")]
    [InlineData("\"2023-06-15 12:00:00+03:00\"")]
    [InlineData("\"2023-12-31 23:59:59-08:00\"")]
    public void Read_VariousValidFormats_ShouldParseCorrectly(string jsonInput)
    {
      // arrange
      var reader = CreateJsonReaderFromString(jsonInput);

      // act & assert - should not throw
      var result = _converter.Read(ref reader, typeof(DateTimeOffset), _options);
      Assert.True(result != default(DateTimeOffset));
    }

    [Fact]
    public void Read_InvalidDateTimeString_ShouldThrowFormatException()
    {
      // arrange
      var json = "\"invalid-date-string\"";

      // act & assert
      Assert.Throws<FormatException>(() =>
      {
        var reader = CreateJsonReaderFromString(json);
        return _converter.Read(ref reader, typeof(DateTimeOffset), _options);
      });
    }

    [Fact]
    public void Read_EmptyString_ShouldThrowFormatException()
    {
      // arrange
      var json = "\"\"";

      // act & assert
      Assert.Throws<FormatException>(() =>
      {
        var reader = CreateJsonReaderFromString(json);
        return _converter.Read(ref reader, typeof(DateTimeOffset), _options);
      });
    }

    [Fact]
    public void Read_NullString_ShouldThrowFormatException()
    {
      // arrange
      var json = "null";

      // act & assert
      Assert.Throws<FormatException>(() =>
      {
        var reader = CreateJsonReaderFromString(json);
        return _converter.Read(ref reader, typeof(DateTimeOffset), _options);
      });
    }
  }

  public class WriteTests : DateTimeOffsetConverterTests
  {
    [Fact]
    public void Write_ValidDateTimeOffset_ShouldWriteCorrectFormat()
    {
      // arrange
      var dateTimeOffset = new DateTimeOffset(2023, 12, 25, 14, 30, 45, TimeSpan.FromHours(2));
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, dateTimeOffset, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal("2023-12-25 14:30:45+02:00", json);
    }

    [Fact]
    public void Write_UtcDateTimeOffset_ShouldWriteWithZeroOffset()
    {
      // arrange
      var dateTimeOffset = new DateTimeOffset(2023, 12, 25, 14, 30, 45, TimeSpan.Zero);
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, dateTimeOffset, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal("2023-12-25 14:30:45+00:00", json);
    }

    [Fact]
    public void Write_NegativeOffsetDateTimeOffset_ShouldWriteCorrectFormat()
    {
      // arrange
      var dateTimeOffset = new DateTimeOffset(2023, 12, 25, 14, 30, 45, TimeSpan.FromHours(-5));
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, dateTimeOffset, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal("2023-12-25 14:30:45-05:00", json);
    }

    [Fact]
    public void Write_MinValue_ShouldWriteCorrectly()
    {
      // arrange
      var dateTimeOffset = DateTimeOffset.MinValue;
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, dateTimeOffset, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.NotNull(json);
      Assert.Contains("0001-01-01", json);
    }

    [Fact]
    public void Write_MaxValue_ShouldWriteCorrectly()
    {
      // arrange
      var dateTimeOffset = DateTimeOffset.MaxValue;
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, dateTimeOffset, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.NotNull(json);
      Assert.Contains("9999-12-31", json);
    }

    [Theory]
    [InlineData(0)]    // UTC
    [InlineData(2)]    // +02:00
    [InlineData(-5)]   // -05:00
    [InlineData(12)]   // +12:00
    [InlineData(-11)]  // -11:00
    public void Write_VariousTimeZoneOffsets_ShouldWriteCorrectFormat(int offsetHours)
    {
      // arrange
      var dateTimeOffset = new DateTimeOffset(2023, 6, 15, 12, 30, 45, TimeSpan.FromHours(offsetHours));
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, dateTimeOffset, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal($"2023-06-15 12:30:45{offsetHours:+00;-00;+00}:00", json);
    }
  }

  public class RoundTripTests : DateTimeOffsetConverterTests
  {
    [Fact]
    public void RoundTrip_WriteAndRead_ShouldReturnOriginalValue()
    {
      // arrange
      var originalValue = new DateTimeOffset(2023, 12, 25, 14, 30, 45, TimeSpan.FromHours(3));

      // act - write
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);
      _converter.Write(writer, originalValue, _options);
      writer.Flush();

      // act - read
      var reader = new Utf8JsonReader(stream.ToArray());
      reader.Read();
      var roundTripValue = _converter.Read(ref reader, typeof(DateTimeOffset), _options);

      // assert
      Assert.Equal(originalValue, roundTripValue);
    }

    [Theory]
    [InlineData(2023, 1, 1, 0, 0, 0, 0)]
    [InlineData(2023, 6, 15, 12, 30, 45, 2)]
    [InlineData(2023, 12, 31, 23, 59, 59, -8)]
    public void RoundTrip_VariousDateTimes_ShouldMaintainOriginalValue(int year, int month, int day, int hour, int minute, int second, int offsetHours)
    {
      // arrange
      var originalValue = new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.FromHours(offsetHours));

      // act - write
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);
      _converter.Write(writer, originalValue, _options);
      writer.Flush();

      // act - read
      var reader = new Utf8JsonReader(stream.ToArray());
      reader.Read();
      var roundTripValue = _converter.Read(ref reader, typeof(DateTimeOffset), _options);

      // assert
      Assert.Equal(originalValue, roundTripValue);
    }

    [Fact]
    public void RoundTrip_DateTimeOffsetNow_ShouldMaintainPrecision()
    {
      // arrange
      var originalValue = DateTimeOffset.Now;

      // act - write
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);
      _converter.Write(writer, originalValue, _options);
      writer.Flush();

      // act - read
      var reader = new Utf8JsonReader(stream.ToArray());
      reader.Read();
      var roundTripValue = _converter.Read(ref reader, typeof(DateTimeOffset), _options);

      // assert - allow for millisecond precision differences due to formatting
      var timeDifference = Math.Abs((originalValue - roundTripValue).TotalMilliseconds);
      Assert.True(timeDifference < 1000, $"Time difference {timeDifference}ms is too large");
      Assert.Equal(originalValue.Offset, roundTripValue.Offset);
    }
  }

  public class FullJsonSerializationTests : DateTimeOffsetConverterTests
  {
    [Fact]
    public void FullSerialization_WithJsonSerializerOptions_ShouldWorkCorrectly()
    {
      // arrange
      var options = new JsonSerializerOptions();
      options.Converters.Add(new DateTimeOffsetConverter());

      var testObject = new TestClass { DateTime = new DateTimeOffset(2023, 12, 25, 14, 30, 45, TimeSpan.FromHours(2)) };

      // act
      var json = JsonSerializer.Serialize(testObject, options);
      var deserializedObject = JsonSerializer.Deserialize<TestClass>(json, options);

      // assert
      Assert.NotNull(deserializedObject);
      Assert.Equal(testObject.DateTime, deserializedObject.DateTime);
    }

    [Fact]
    public void FullSerialization_ArrayOfDateTimeOffsets_ShouldWorkCorrectly()
    {
      // arrange
      var options = new JsonSerializerOptions();
      options.Converters.Add(new DateTimeOffsetConverter());

      var dates = new[]
      {
        new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2023, 6, 15, 12, 30, 45, TimeSpan.FromHours(3)),
        new DateTimeOffset(2023, 12, 31, 23, 59, 59, TimeSpan.FromHours(-5))
      };

      // act
      var json = JsonSerializer.Serialize(dates, options);
      var deserializedDates = JsonSerializer.Deserialize<DateTimeOffset[]>(json, options);

      // assert
      Assert.NotNull(deserializedDates);
      Assert.Equal(dates.Length, deserializedDates.Length);
      for (int i = 0; i < dates.Length; i++)
      {
        Assert.Equal(dates[i], deserializedDates[i]);
      }
    }

    private class TestClass
    {
      public DateTimeOffset DateTime { get; set; }
    }
  }
}
