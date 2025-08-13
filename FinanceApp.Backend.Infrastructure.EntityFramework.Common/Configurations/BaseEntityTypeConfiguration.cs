using FinanceApp.Backend.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Configurations;

public abstract class BaseEntityTypeConfiguration<TDerived> : IEntityTypeConfiguration<TDerived> where TDerived : BaseEntity
{
  public void Configure(EntityTypeBuilder<TDerived> builder)
  {
    builder.HasKey(e => e.Id);

    ConfigureSpecificProperties(builder);
  }

  protected abstract void ConfigureSpecificProperties(EntityTypeBuilder<TDerived> builder);
}
