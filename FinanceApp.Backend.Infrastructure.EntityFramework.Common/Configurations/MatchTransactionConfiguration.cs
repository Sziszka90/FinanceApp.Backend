using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Configurations;

public class MatchTransactionConfiguration : BaseEntityTypeConfiguration<MatchTransaction>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<MatchTransaction> builder)
  {
    builder.ToTable("MatchTransaction");

    builder.HasKey(m => new { m.Transaction, m.TransactionGroup });

    builder.Property(m => m.Transaction)
      .IsRequired()
      .HasMaxLength(255);

    builder.Property(m => m.TransactionGroup)
      .IsRequired()
      .HasMaxLength(255);
  }
}
