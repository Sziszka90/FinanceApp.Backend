using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Saving.SavingQueries;

public record GetSavingByNameQuery(Guid Id) : IQuery<Result<GetSavingDto>>;