using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class InvestmentConfiguration : BaseEntityTypeConfiguration<Investment>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<Investment> builder)
  {
    builder.ToTable(nameof(Investment));
    builder.OwnsOne(x => x.Value, money =>
    {
      money.Property(m => m.Amount).HasColumnName("Value_Amount");
      money.Property(m => m.Currency).HasColumnName("Value_Currency");
    });
  }
}
