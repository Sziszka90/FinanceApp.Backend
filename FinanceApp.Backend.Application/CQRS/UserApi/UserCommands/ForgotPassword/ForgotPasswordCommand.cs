using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.ForgotPassword;

public record ForgotPasswordCommand(EmailDto EmailDto, CancellationToken CancellationToken) : ICommand<Result>;
