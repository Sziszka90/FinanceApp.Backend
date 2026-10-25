using FinanceApp.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Configurations;

public class CategorizationRuleConfiguration : BaseEntityTypeConfiguration<CategorizationRule>
{
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<CategorizationRule> builder)
  {
    builder.ToTable(nameof(CategorizationRule));
    builder.Property(rule => rule.MerchantText).HasMaxLength(200).IsRequired();
    builder.HasOne(rule => rule.User).WithMany().HasForeignKey(rule => rule.UserId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(rule => rule.TransactionGroup).WithMany().HasForeignKey(rule => rule.TransactionGroupId)
      .OnDelete(DeleteBehavior.Cascade);
    builder.HasIndex(rule => rule.UserId);
  }
}
