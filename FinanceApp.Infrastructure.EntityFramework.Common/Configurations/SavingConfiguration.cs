using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class SavingConfiguration : BaseEntityTypeConfiguration<Saving>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<Saving> entity)
  {
    entity.ToTable(nameof(Saving));
    entity.ComplexProperty(y => y.Amount, y => { y.IsRequired(); });
  }
}
