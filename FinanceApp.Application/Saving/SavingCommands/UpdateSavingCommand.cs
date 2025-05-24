using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.SavingDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Saving.SavingCommands;

public record UpdateSavingCommand(UpdateSavingDto UpdateSavingDto) : ICommand<Result<GetSavingDto>>;
