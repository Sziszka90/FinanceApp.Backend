using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class TransactionGroupConfiguration : IEntityTypeConfiguration<TransactionGroup>
{
  /// <inheritdoc />
  public void Configure(EntityTypeBuilder<TransactionGroup> builder)
  {}
}
