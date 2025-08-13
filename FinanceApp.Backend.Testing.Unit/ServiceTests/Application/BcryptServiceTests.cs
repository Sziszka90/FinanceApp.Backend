using FinanceApp.Backend.Application.Services;

namespace FinanceApp.Backend.Testing.Unit.ServiceTests.Application;

public class BcryptServiceTests
{
  private readonly BcryptService _service = new();

  [Fact]
  public void Hash_And_Verify_WorksCorrectly()
  {
    // arrange
    var password = "myPassword123";
    var hash = _service.Hash(password);

    // act
    var isVerified = _service.Verify(password, hash);
    var isNotVerified = _service.Verify("wrongPassword", hash);

    // assert
    Assert.True(isVerified);
    Assert.False(isNotVerified);
  }
}
