using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class UserConfiguration : BaseEntityTypeConfiguration<Domain.Entities.User>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<Domain.Entities.User> builder)
  {
    builder.ToTable(nameof(User));
  }
}
