using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.ResendConfirmationEmail;

public record ResendConfirmationEmailCommand(EmailDto EmailDto, CancellationToken CancellationToken) : ICommand<Result<ResendEmailConfirmationResponse>>;
