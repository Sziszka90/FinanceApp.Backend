using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.UserApi.UserCommands.ResendConfirmationEmail;

public record ResendConfirmationEmailCommand(EmailDto EmailDto, CancellationToken CancellationToken) : ICommand<Result<ResendEmailConfirmationResponse>>;
