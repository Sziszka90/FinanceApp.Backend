
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Backend.Application.Dtos.TransactionDtos;

public class UploadCsvFileDto
{
  [FromForm(Name = "file")]
  public IFormFile File { get; set; } = null!;

  [FromForm(Name = "correlationId")]
  public string CorrelationId { get; set; } = string.Empty;
}
