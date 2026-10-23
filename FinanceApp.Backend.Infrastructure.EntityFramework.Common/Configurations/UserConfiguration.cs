using FinanceApp.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Configurations;

public class UserConfiguration : BaseEntityTypeConfiguration<User>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<User> builder)
  {
    builder.ToTable(nameof(User));
  }
}
