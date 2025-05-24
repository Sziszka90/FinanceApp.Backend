using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class InvestmentConfiguration : BaseEntityTypeConfiguration<Investment>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<Investment> entity)
  {
    entity.ToTable(nameof(Investment));
    entity.ComplexProperty(y => y.Amount, y => { y.IsRequired(); });
  }
}
