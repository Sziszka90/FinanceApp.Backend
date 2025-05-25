using FinanceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinanceApp.Infrastructure.EntityFramework.Interceptors;

public class TimestampableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    UpdateEntitie(eventData.Context);
    return base.SavingChanges(eventData, result);
  }

  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = new())
  {
    UpdateEntitie(eventData.Context);
    return base.SavingChangesAsync(eventData, result, cancellationToken);
  }

  private static void UpdateEntitie(DbContext? context)
  {
    if (context is null)
    {
      return;
    }

    foreach (var entry in context.ChangeTracker.Entries<ITimestampable>())
    {
      if (entry.Entity is { } timestampableEntity)
      {
        switch (entry.State)
        {
          case EntityState.Added:
            break;
          case EntityState.Modified:
            timestampableEntity.UpdateModifiedDate();
            break;
          case EntityState.Detached:
            break;
          case EntityState.Unchanged:
            break;
          case EntityState.Deleted:
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }
  }
}
