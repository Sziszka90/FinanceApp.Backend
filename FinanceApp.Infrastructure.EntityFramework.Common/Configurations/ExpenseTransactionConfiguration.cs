using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class ExpenseTransactionConfiguration : BaseEntityTypeConfiguration<ExpenseTransaction>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<ExpenseTransaction> entity)
  {
    entity.ToTable(nameof(ExpenseTransaction));
    entity.ComplexProperty(y => y.Value, y => { y.IsRequired(); });
  }
}