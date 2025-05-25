using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class TransactionConfiguration : BaseEntityTypeConfiguration<Transaction>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<Transaction> builder)
  {
    builder.ToTable(nameof(Transaction));
    builder.OwnsOne(e => e.Value, owned =>
        {
          owned.Property(v => v.Amount).HasColumnName("Amount");
          owned.Property(v => v.Currency).HasColumnName("Currency");
        });
  }
}
