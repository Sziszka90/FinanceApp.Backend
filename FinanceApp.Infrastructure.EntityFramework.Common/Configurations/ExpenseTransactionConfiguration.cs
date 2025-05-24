using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class ExpenseTransactionConfiguration : BaseEntityTypeConfiguration<ExpenseTransaction>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<ExpenseTransaction> builder)
  {
    builder.ToTable(nameof(ExpenseTransaction));
    builder.OwnsOne(e => e.Value, owned =>
        {
          owned.Property(v => v.Amount).HasColumnName("Amount");
          owned.Property(v => v.Currency).HasColumnName("Currency");
        });
  }
}
