using System.Globalization;
using System.Text;
using System.Text.Json;
using FinanceApp.Backend.Application.Converters;

namespace FinanceApp.Backend.Testing.Unit.ConverterTests;

public class DecimalConverterTests
{
  protected readonly DecimalConverter _converter;
  protected readonly JsonSerializerOptions _options;

  public DecimalConverterTests()
  {
    _converter = new DecimalConverter();
    _options = new JsonSerializerOptions();
  }

  protected static Utf8JsonReader CreateJsonReaderFromString(string json)
  {
    var bytes = Encoding.UTF8.GetBytes(json);
    var reader = new Utf8JsonReader(bytes);
    reader.Read(); // Move to the first token
    return reader;
  }

  protected static Utf8JsonReader CreateJsonReaderFromRawValue(string json)
  {
    var bytes = Encoding.UTF8.GetBytes(json);
    var reader = new Utf8JsonReader(bytes);
    reader.Read(); // Move to the value token
    return reader;
  }

  public class ReadTests : DecimalConverterTests
  {
    [Fact]
    public void Read_ValidDecimalAsString_ShouldReturnCorrectDecimal()
    {
      // arrange
      var json = "\"123.45\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(123.45m, result);
    }

    [Fact]
    public void Read_ValidDecimalAsNumber_ShouldReturnCorrectDecimal()
    {
      // arrange
      var json = "123.45";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(123.45m, result);
    }

    [Fact]
    public void Read_IntegerAsString_ShouldReturnCorrectDecimal()
    {
      // arrange
      var json = "\"42\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(42m, result);
    }

    [Fact]
    public void Read_IntegerAsNumber_ShouldReturnCorrectDecimal()
    {
      // arrange
      var json = "42";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(42m, result);
    }

    [Fact]
    public void Read_ZeroAsString_ShouldReturnZero()
    {
      // arrange
      var json = "\"0\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(0m, result);
    }

    [Fact]
    public void Read_ZeroAsNumber_ShouldReturnZero()
    {
      // arrange
      var json = "0";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(0m, result);
    }

    [Fact]
    public void Read_NegativeDecimalAsString_ShouldReturnCorrectDecimal()
    {
      // arrange
      var json = "\"-123.45\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(-123.45m, result);
    }

    [Fact]
    public void Read_NegativeDecimalAsNumber_ShouldReturnCorrectDecimal()
    {
      // arrange
      var json = "-123.45";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(-123.45m, result);
    }

    [Fact]
    public void Read_VeryLargeDecimalAsString_ShouldReturnCorrectDecimal()
    {
      // arrange
      var largeDecimal = "999999999999999999999999999.99";
      var json = $"\"{largeDecimal}\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(decimal.Parse(largeDecimal, CultureInfo.InvariantCulture), result);
    }

    [Fact]
    public void Read_VerySmallDecimalAsString_ShouldReturnCorrectDecimal()
    {
      // arrange
      var smallDecimal = "0.0000000000000000000000000001";
      var json = $"\"{smallDecimal}\"";
      var reader = CreateJsonReaderFromString(json);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(decimal.Parse(smallDecimal, CultureInfo.InvariantCulture), result);
    }

    [Theory]
    [InlineData("\"0\"", 0)]
    [InlineData("\"1\"", 1)]
    [InlineData("\"1.5\"", 1.5)]
    [InlineData("\"-1.5\"", -1.5)]
    [InlineData("\"100.00\"", 100.00)]
    [InlineData("\"0.01\"", 0.01)]
    public void Read_VariousValidStringDecimals_ShouldReturnCorrectValues(string jsonInput, double expectedValue)
    {
      // arrange
      var reader = CreateJsonReaderFromString(jsonInput);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal((decimal)expectedValue, result);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("1.5", 1.5)]
    [InlineData("-1.5", -1.5)]
    [InlineData("100.00", 100.00)]
    [InlineData("0.01", 0.01)]
    public void Read_VariousValidNumberDecimals_ShouldReturnCorrectValues(string jsonInput, double expectedValue)
    {
      // arrange
      var reader = CreateJsonReaderFromString(jsonInput);

      // act
      var result = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal((decimal)expectedValue, result);
    }

    [Fact]
    public void Read_InvalidStringDecimal_ShouldThrowJsonException()
    {
      // arrange
      var json = "\"invalid-decimal\"";

      // act & assert
      var exception = Assert.Throws<JsonException>(() =>
      {
        var reader = CreateJsonReaderFromString(json);
        return _converter.Read(ref reader, typeof(decimal), _options);
      });
      Assert.Contains("invalid-decimal", exception.Message);
    }

    [Fact]
    public void Read_EmptyString_ShouldThrowJsonException()
    {
      // arrange
      var json = "\"\"";

      // act & assert
      var exception = Assert.Throws<JsonException>(() =>
      {
        var reader = CreateJsonReaderFromString(json);
        return _converter.Read(ref reader, typeof(decimal), _options);
      });
      Assert.Contains("Unable to convert", exception.Message);
    }
  }

  public class WriteTests : DecimalConverterTests
  {
    [Fact]
    public void Write_NegativeDecimal_ShouldWriteCorrectString()
    {
      // arrange
      var value = -123.45m;
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, value, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal("-123.45", json);
    }

    [Fact]
    public void Write_Zero_ShouldWriteZeroString()
    {
      // arrange
      var value = 0m;
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, value, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal("0", json);
    }

    [Fact]
    public void Write_Integer_ShouldWriteIntegerString()
    {
      // arrange
      var value = 42m;
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, value, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal("42", json);
    }

    [Fact]
    public void Write_VeryLargeDecimal_ShouldWriteCorrectString()
    {
      // arrange
      var value = 999999999999999999999999999.99m;
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, value, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal(value.ToString(CultureInfo.InvariantCulture), json);
    }

    [Fact]
    public void Write_VerySmallDecimal_ShouldWriteCorrectString()
    {
      // arrange
      var value = 0.0000000000000000000000000001m;
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, value, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal(value.ToString(CultureInfo.InvariantCulture), json);
    }

    [Fact]
    public void Write_MaxValue_ShouldWriteCorrectString()
    {
      // arrange
      var value = decimal.MaxValue;
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, value, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal(decimal.MaxValue.ToString(CultureInfo.InvariantCulture), json);
    }

    [Fact]
    public void Write_MinValue_ShouldWriteCorrectString()
    {
      // arrange
      var value = decimal.MinValue;
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, value, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal(decimal.MinValue.ToString(CultureInfo.InvariantCulture), json);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(1.5)]
    [InlineData(-1.5)]
    [InlineData(100.00)]
    [InlineData(0.01)]
    [InlineData(999.999)]
    public void Write_VariousDecimals_ShouldWriteCorrectStrings(double inputValue)
    {
      // arrange
      var value = (decimal)inputValue;
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);

      // act
      _converter.Write(writer, value, _options);
      writer.Flush();

      // assert
      var json = JsonSerializer.Deserialize<string>(stream.ToArray());
      Assert.Equal(value.ToString(CultureInfo.InvariantCulture), json);
    }
  }

  public class RoundTripTests : DecimalConverterTests
  {
    [Fact]
    public void RoundTrip_WriteAndReadString_ShouldReturnOriginalValue()
    {
      // arrange
      var originalValue = 123.45m;

      // act - write
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);
      _converter.Write(writer, originalValue, _options);
      writer.Flush();

      // act - read (the written value will be a string)
      var jsonString = JsonSerializer.Deserialize<string>(stream.ToArray());
      var quotedJson = $"\"{jsonString}\"";
      var reader = CreateJsonReaderFromString(quotedJson);
      var roundTripValue = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(originalValue, roundTripValue);
    }

    [Fact]
    public void RoundTrip_WriteAndReadNumber_ShouldReturnOriginalValue()
    {
      // arrange
      var originalValue = 123.45m;

      // act - serialize as number directly
      var jsonNumber = originalValue.ToString(CultureInfo.InvariantCulture);
      var reader = CreateJsonReaderFromString(jsonNumber);
      var roundTripValue = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(originalValue, roundTripValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(1.5)]
    [InlineData(-1.5)]
    [InlineData(100.00)]
    [InlineData(0.01)]
    [InlineData(999.999)]
    [InlineData(12345.6789)]
    public void RoundTrip_VariousDecimals_ShouldMaintainOriginalValue(double inputValue)
    {
      // arrange
      var originalValue = (decimal)inputValue;

      // act - test string round trip
      using var stream = new MemoryStream();
      using var writer = new Utf8JsonWriter(stream);
      _converter.Write(writer, originalValue, _options);
      writer.Flush();

      var jsonString = JsonSerializer.Deserialize<string>(stream.ToArray());
      var quotedJson = $"\"{jsonString}\"";
      var reader = CreateJsonReaderFromString(quotedJson);
      var roundTripValue = _converter.Read(ref reader, typeof(decimal), _options);

      // assert
      Assert.Equal(originalValue, roundTripValue);
    }

    [Fact]
    public void RoundTrip_ExtremeValues_ShouldMaintainPrecision()
    {
      // arrange
      var extremeValues = new[]
      {
        decimal.MaxValue,
        decimal.MinValue,
        0m,
        0.0000000000000000000000000001m,
        999999999999999999999999999.99m
      };

      foreach (var originalValue in extremeValues)
      {
        // act - write
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        _converter.Write(writer, originalValue, _options);
        writer.Flush();

        // act - read
        var jsonString = JsonSerializer.Deserialize<string>(stream.ToArray());
        var quotedJson = $"\"{jsonString}\"";
        var reader = CreateJsonReaderFromString(quotedJson);
        var roundTripValue = _converter.Read(ref reader, typeof(decimal), _options);

        // assert
        Assert.Equal(originalValue, roundTripValue);
      }
    }
  }

  public class FullJsonSerializationTests : DecimalConverterTests
  {
    [Fact]
    public void FullSerialization_WithJsonSerializerOptions_ShouldWorkCorrectly()
    {
      // arrange
      var options = new JsonSerializerOptions();
      options.Converters.Add(new DecimalConverter());

      var testObject = new TestClass { Amount = 123.45m };

      // act
      var json = JsonSerializer.Serialize(testObject, options);
      var deserializedObject = JsonSerializer.Deserialize<TestClass>(json, options);

      // assert
      Assert.NotNull(deserializedObject);
      Assert.Equal(testObject.Amount, deserializedObject.Amount);
    }

    [Fact]
    public void FullSerialization_ArrayOfDecimals_ShouldWorkCorrectly()
    {
      // arrange
      var options = new JsonSerializerOptions();
      options.Converters.Add(new DecimalConverter());

      var amounts = new[] { 0m, 123.45m, -67.89m, 999.999m };

      // act
      var json = JsonSerializer.Serialize(amounts, options);
      var deserializedAmounts = JsonSerializer.Deserialize<decimal[]>(json, options);

      // assert
      Assert.NotNull(deserializedAmounts);
      Assert.Equal(amounts.Length, deserializedAmounts.Length);
      for (int i = 0; i < amounts.Length; i++)
      {
        Assert.Equal(amounts[i], deserializedAmounts[i]);
      }
    }

    [Fact]
    public void FullSerialization_ComplexObjectWithMultipleDecimals_ShouldWorkCorrectly()
    {
      // arrange
      var options = new JsonSerializerOptions();
      options.Converters.Add(new DecimalConverter());

      var testObject = new ComplexTestClass
      {
        Price = 123.45m,
        Tax = 12.34m,
        Discount = 0.15m,
        Total = 135.79m
      };

      // act
      var json = JsonSerializer.Serialize(testObject, options);
      var deserializedObject = JsonSerializer.Deserialize<ComplexTestClass>(json, options);

      // assert
      Assert.NotNull(deserializedObject);
      Assert.Equal(testObject.Price, deserializedObject.Price);
      Assert.Equal(testObject.Tax, deserializedObject.Tax);
      Assert.Equal(testObject.Discount, deserializedObject.Discount);
      Assert.Equal(testObject.Total, deserializedObject.Total);
    }

    [Fact]
    public void FullSerialization_MixedNumberAndStringInputs_ShouldWorkCorrectly()
    {
      // arrange
      var options = new JsonSerializerOptions();
      options.Converters.Add(new DecimalConverter());

      // Manually create JSON with mixed number and string representations
      var mixedJson = "{\"StringAmount\":\"123.45\",\"NumberAmount\":67.89}";

      // act
      var deserializedObject = JsonSerializer.Deserialize<MixedTestClass>(mixedJson, options);

      // assert
      Assert.NotNull(deserializedObject);
      Assert.Equal(123.45m, deserializedObject.StringAmount);
      Assert.Equal(67.89m, deserializedObject.NumberAmount);
    }

    private class TestClass
    {
      public decimal Amount { get; set; }
    }

    private class ComplexTestClass
    {
      public decimal Price { get; set; }
      public decimal Tax { get; set; }
      public decimal Discount { get; set; }
      public decimal Total { get; set; }
    }

    private class MixedTestClass
    {
      public decimal StringAmount { get; set; }
      public decimal NumberAmount { get; set; }
    }
  }

  public class CultureInvariantTests : DecimalConverterTests
  {
    [Fact]
    public void Read_InvariantCulture_ShouldParseCorrectly()
    {
      // arrange
      var currentCulture = CultureInfo.CurrentCulture;
      try
      {
        // Set to a culture that uses comma as decimal separator
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");

        var json = "\"123.45\""; // Using dot as decimal separator
        var reader = CreateJsonReaderFromString(json);

        // act
        var result = _converter.Read(ref reader, typeof(decimal), _options);

        // assert
        Assert.Equal(123.45m, result);
      }
      finally
      {
        CultureInfo.CurrentCulture = currentCulture;
      }
    }

    [Fact]
    public void Write_InvariantCulture_ShouldWriteWithDot()
    {
      // arrange
      var currentCulture = CultureInfo.CurrentCulture;
      try
      {
        // Set to a culture that uses comma as decimal separator
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");

        var value = 123.45m;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // act
        _converter.Write(writer, value, _options);
        writer.Flush();

        // assert
        var json = JsonSerializer.Deserialize<string>(stream.ToArray());
        Assert.Equal("123.45", json); // Should use dot, not comma
      }
      finally
      {
        CultureInfo.CurrentCulture = currentCulture;
      }
    }
  }
}
