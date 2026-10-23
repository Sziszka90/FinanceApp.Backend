using FinanceApp.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Configurations;

public class BankStatementPromptConfiguration : BaseEntityTypeConfiguration<BankStatementPrompt>
{
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<BankStatementPrompt> builder)
  {
    builder.ToTable(nameof(BankStatementPrompt));
    builder.Property(prompt => prompt.Name).HasMaxLength(100).IsRequired();
    builder.Property(prompt => prompt.Instructions).HasMaxLength(4000).IsRequired();
    builder.HasOne(prompt => prompt.User)
      .WithMany()
      .HasForeignKey(prompt => prompt.UserId)
      .OnDelete(DeleteBehavior.Cascade);
    builder.HasIndex(prompt => new { prompt.UserId, prompt.IsSelected });
  }
}
