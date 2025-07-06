using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Dtos.SaltEdgeDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Clients.HttpClients;

public class SaltEdgeClient : HttpClientBase, ISaltEdgeClient
{

  public SaltEdgeClient(HttpClient httpClient) : base(httpClient)
  {}

  public async Task<Result<CreateConnectionResponseDto>> CreateConnectionAsync(CreateConnectionRequestDto createConnectionRequestDto)
  {
    var response = await PostAsync<CreateConnectionRequestDto, CreateConnectionResponseDto>("/connections/connect", createConnectionRequestDto);
    return response != null ? Result.Success(response) :
    Result.Failure<CreateConnectionResponseDto>(ApplicationError.SaltEdgeUserConnectionError());
  }

  public async Task<Result<CreateUserDataResponseDto>> CreateUserAsync(CreateUserDataRequestDto createUserDataRequestDto)
  {
    var response = await PostAsync<CreateUserDataRequestDto, CreateUserDataResponseDto>("/customers", createUserDataRequestDto);
    return response != null ? Result.Success(response) :
    Result.Failure<CreateUserDataResponseDto>(ApplicationError.SaltEdgeUserCreationError(createUserDataRequestDto.Data.Identifier));
  }
}
