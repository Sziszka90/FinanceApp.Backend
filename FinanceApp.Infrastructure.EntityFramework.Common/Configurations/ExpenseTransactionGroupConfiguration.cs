using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class ExpenseTransactionGroupConfiguration : IEntityTypeConfiguration<ExpenseTransactionGroup>
{
  #region Methods

  /// <inheritdoc />
  public void Configure(EntityTypeBuilder<ExpenseTransactionGroup> builder)
  {
    builder.ComplexProperty(y => y.Limit, y => { y.IsRequired(); });
  }

  #endregion
}
