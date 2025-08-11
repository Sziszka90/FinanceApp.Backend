using FinanceApp.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Configurations;

public class TransactionConfiguration : BaseEntityTypeConfiguration<Transaction>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<Transaction> builder)
  {
    builder.ToTable(nameof(Transaction));
    builder.OwnsOne(e => e.Value, owned =>
      {
        owned.Property(v => v.Amount).HasColumnName("Amount");
        owned.Property(v => v.Currency).HasColumnName("Currency");
      });

    builder.HasOne(t => t.User)
           .WithMany()
           .HasForeignKey(t => t.UserId)
           .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(t => t.TransactionGroup)
           .WithMany()
           .HasForeignKey(t => t.TransactionGroupId)
           .OnDelete(DeleteBehavior.SetNull);
  }
}
