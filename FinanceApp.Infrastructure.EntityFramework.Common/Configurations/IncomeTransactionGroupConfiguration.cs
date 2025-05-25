using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class IncomeTransactionGroupConfiguration : IEntityTypeConfiguration<IncomeTransactionGroup>
{
  /// <inheritdoc />
  public void Configure(EntityTypeBuilder<IncomeTransactionGroup> builder) { }
}
