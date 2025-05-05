using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class IncomeTransactionConfiguration : BaseEntityTypeConfiguration<IncomeTransaction>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<IncomeTransaction> entity)
  {
    entity.ToTable(nameof(IncomeTransaction));
    entity.ComplexProperty(y => y.Value, y => { y.IsRequired(); });
  }
}