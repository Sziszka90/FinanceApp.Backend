namespace FinanceApp.Application.Abstraction.Repositories;

public interface IUserRepository : IRepository<Domain.Entities.User>
{
  #region Methods

  public Task<Domain.Entities.User?> GetByUserNameAsync(string userName, bool noTracking = false, CancellationToken cancellationToken = default);

  #endregion
}