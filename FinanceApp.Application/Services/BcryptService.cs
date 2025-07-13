
using FinanceApp.Application.Abstraction.Services;

namespace FinanceApp.Application.Services;

public class BcryptService : IBcryptService
{
  public bool Verify(string password, string hash)
      => BCrypt.Net.BCrypt.Verify(password, hash);

  public string Hash(string password)
      => BCrypt.Net.BCrypt.HashPassword(password);
}
