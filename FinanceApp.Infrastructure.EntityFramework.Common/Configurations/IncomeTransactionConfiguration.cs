using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class IncomeTransactionConfiguration : BaseEntityTypeConfiguration<IncomeTransaction>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<IncomeTransaction> builder)
  {
    builder.ToTable(nameof(IncomeTransaction));
    builder.OwnsOne(e => e.Value, owned =>
        {
          owned.Property(v => v.Amount).HasColumnName("Amount");
          owned.Property(v => v.Currency).HasColumnName("Currency");
        });
  }
}
