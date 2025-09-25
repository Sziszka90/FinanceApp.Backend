using System.Data.Common;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Extensions;

public static class DbDataReaderExtensions
{
  public static string? GetNullableString(this DbDataReader reader, int ordinal)
  {
    return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
  }

  public static T? GetNullableValue<T>(this DbDataReader reader, int ordinal) where T : struct
  {
    return reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<T>(ordinal);
  }

  public static T GetValueOrDefault<T>(this DbDataReader reader, int ordinal, T defaultValue = default!)
  {
    return reader.IsDBNull(ordinal) ? defaultValue : reader.GetFieldValue<T>(ordinal);
  }

  public static DateTimeOffset GetDateTimeOffsetSafe(this DbDataReader reader, int ordinal)
  {
    var value = reader.GetValue(ordinal);
    if (value is DateTimeOffset dto)
    {
      return dto;
    }
    if (value is DateTime dt)
    {
      return dt.Kind == DateTimeKind.Local ? new DateTimeOffset(dt) : new DateTimeOffset(dt, TimeSpan.Zero);
    }
    throw new InvalidCastException($"Column at ordinal {ordinal} is not a DateTimeOffset or DateTime.");
  }
}
