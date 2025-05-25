using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class TransactionGroupConfiguration : IEntityTypeConfiguration<TransactionGroup>
{
  /// <inheritdoc />
  public void Configure(EntityTypeBuilder<TransactionGroup> builder)
  {
        builder.OwnsOne(e => e.Limit, owned =>
        {
          owned.Property(v => v.Amount).HasColumnName("Limit_Amount");
          owned.Property(v => v.Currency).HasColumnName("Limit_Currency");
        });
  }
}
