using FinanceApp.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Configurations;

public class TransactionGroupConfiguration : BaseEntityTypeConfiguration<TransactionGroup>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<TransactionGroup> builder)
  {
    builder.ToTable(nameof(TransactionGroup));
  }
}
