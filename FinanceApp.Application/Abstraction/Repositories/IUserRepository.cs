namespace FinanceApp.Application.Abstraction.Repositories;

public interface IUserRepository : IRepository<Domain.Entities.User>
{
  public Task<Domain.Entities.User?> GetByUserNameAsync(string userName, bool noTracking = false, CancellationToken cancellationToken = default);
  public Task<Domain.Entities.User?> GetUserByEmailAsync(string email, bool noTracking = false, CancellationToken cancellationToken = default);
}
