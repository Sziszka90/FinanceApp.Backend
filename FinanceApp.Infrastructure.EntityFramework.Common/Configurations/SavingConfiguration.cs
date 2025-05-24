using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class SavingConfiguration : BaseEntityTypeConfiguration<Saving>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<Saving> builder)
  {
    builder.ToTable(nameof(Saving));
    builder.OwnsOne(x => x.Value, money =>
    {
      money.Property(m => m.Amount).HasColumnName("Value_Amount");
      money.Property(m => m.Currency).HasColumnName("Value_Currency");
    });
  }
}
