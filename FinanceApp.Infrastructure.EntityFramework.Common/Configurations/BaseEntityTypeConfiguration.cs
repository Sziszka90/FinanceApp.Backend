using FinanceApp.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public abstract class BaseEntityTypeConfiguration<TDerived> : IEntityTypeConfiguration<TDerived> where TDerived : BaseEntity
{
  #region Methods

  public void Configure(EntityTypeBuilder<TDerived> builder)
  {
    builder.HasKey(e => e.Id);

    ConfigureSpecificProperties(builder);
  }

  #endregion

  protected abstract void ConfigureSpecificProperties(EntityTypeBuilder<TDerived> entity);
}