using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Options;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.ImportPdf;

public class ImportPdfCommandHandler : ICommandHandler<ImportPdfCommand, Result<string>>
{
  private readonly IUserService _userService;
  private readonly IRabbitMqClient _rabbitMq;
  private readonly IRepository<BankStatementPrompt> _prompts;
  private readonly RabbitMqSettings _settings;

  public ImportPdfCommandHandler(IUserService userService, IRabbitMqClient rabbitMq,
    IRepository<BankStatementPrompt> prompts, IOptions<RabbitMqSettings> settings)
  {
    _userService = userService;
    _rabbitMq = rabbitMq;
    _prompts = prompts;
    _settings = settings.Value;
  }

  public async Task<Result<string>> Handle(ImportPdfCommand request, CancellationToken cancellationToken)
  {
    var user = await _userService.GetActiveUserAsync(cancellationToken);
    if (!user.IsSuccess)
    {
      return Result.Failure<string>(user.ApplicationError!);
    }

    var selectedPrompt = await _prompts.GetFirstOrDefaultAsync(prompt =>
      prompt.UserId == user.Data!.Id && prompt.IsSelected, noTracking: true, cancellationToken: cancellationToken);

    await using var stream = request.Upload.File.OpenReadStream();
    using var buffer = new MemoryStream();
    await stream.CopyToAsync(buffer, cancellationToken);
    var bytes = buffer.ToArray();
    if (bytes.Length > 10 * 1024 * 1024 || bytes.Length < 5 ||
        bytes[0] != '%' || bytes[1] != 'P' || bytes[2] != 'D' || bytes[3] != 'F' || bytes[4] != '-')
    {
      return Result.Failure<string>(ApplicationError.InvalidFileTypeError("PDF"));
    }

    var job = new PdfImportJobDto
    {
      CorrelationId = request.Upload.CorrelationId,
      UserId = user.Data!.Id.ToString(),
      Filename = "statement.pdf",
      PdfBase64 = Convert.ToBase64String(bytes),
      PromptId = selectedPrompt?.Id.ToString(),
      PromptName = selectedPrompt?.Name,
      Prompt = selectedPrompt?.Instructions
    };
    await _rabbitMq.PublishToQueueAsync(_settings.PdfRequestQueue, JsonSerializer.Serialize(job), cancellationToken);
    return Result.Success(job.CorrelationId);
  }
}
