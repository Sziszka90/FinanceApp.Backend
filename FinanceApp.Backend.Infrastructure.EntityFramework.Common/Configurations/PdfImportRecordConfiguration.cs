using FinanceApp.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Configurations;

public class PdfImportRecordConfiguration : BaseEntityTypeConfiguration<PdfImportRecord>
{
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<PdfImportRecord> builder)
  {
    builder.ToTable(nameof(PdfImportRecord));
    builder.HasIndex(record => record.UserId);
    builder.HasOne<User>()
      .WithMany()
      .HasForeignKey(record => record.UserId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
