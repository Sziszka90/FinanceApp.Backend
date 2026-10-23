using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptQueries;

public record GetBankStatementPromptsQuery() : IQuery<Result<List<BankStatementPromptDto>>>;
