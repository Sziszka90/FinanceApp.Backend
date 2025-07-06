using FinanceApp.Application.Dtos.SaltEdgeDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Abstraction.Clients;

public interface ISaltEdgeClient
{
  /// <summary>
  /// Creates a new user in the Salt Edge system.
  /// </summary>
  /// <returns>Task<Result<CreatedUserDto>></returns>
  public Task<Result<CreateUserDataResponseDto>> CreateUserAsync(CreateUserDataRequestDto createUserDataDto);

  /// <summary>
  /// Creates a new connection in the Salt Edge system.
  /// </summary>
  /// <param name="createConnectionDto"></param>
  /// <returns></returns>
  public Task<Result<CreateConnectionResponseDto>> CreateConnectionAsync(CreateConnectionRequestDto createConnectionDto);


  public Task<Result<GetAccountsResponseDto>> GetAccountsAsync(GetAccountsRequestDto getAccountsRequestDto);
}
