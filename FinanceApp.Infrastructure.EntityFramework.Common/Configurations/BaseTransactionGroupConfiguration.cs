using FinanceApp.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class BaseTransactionGroupConfiguration : BaseEntityTypeConfiguration<BaseTransactionGroup>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<BaseTransactionGroup> entity)
  {
    entity.ToTable("TransactionGroup");
    entity.HasDiscriminator<string>("GroupType");
  }
}