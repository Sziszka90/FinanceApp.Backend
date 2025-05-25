using System.Data;
using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class Icon : BaseEntity
{
  /// <summary>
  /// File name of the Transaction Group Icon
  /// </summary>
  public string FileName { get; set; } = String.Empty;

  /// <summary>
  /// Content Type of the Icon
  /// </summary>
  public string ContentType { get; set; } = String.Empty;

  /// <summary>
  /// Data in binary format
  /// </summary>
  public byte[] Data { get; set; } = [];

  public Icon(string fileName, string contentType, byte[] data)
  {
    FileName = fileName;
    ContentType = contentType;
    Data = data;
  }

  public void Update(string fileName, string contentType, byte[] data)
  {
    FileName = fileName;
    ContentType = contentType;
    Data = data;
  }

  private Icon() { }

}
