using FinanceApp.Backend.Domain.Common;

namespace FinanceApp.Backend.Domain.Entities;

public class PdfImportRecord : BaseEntity
{
  public Guid UserId { get; set; }

  public PdfImportRecord(Guid id, Guid userId)
  {
    Id = id;
    UserId = userId;
  }

#pragma warning disable CS8618
  protected PdfImportRecord() { }
}
