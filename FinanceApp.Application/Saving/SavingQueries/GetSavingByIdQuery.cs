using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Saving.SavingQueries;

public record GetSavingByIdQuery(Guid Id) : IQuery<Result<GetSavingDto>>;