using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.SaltEdgeDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.User.UserCommands;

public record CreateConnectionWithBankCommand() : ICommand<Result<CreateConnectionResponseDto>>;
