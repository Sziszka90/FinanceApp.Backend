
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Application.Dtos.TransactionDtos;

public class UploadCsvFileDto
{
  [FromForm(Name = "file")]
  public IFormFile File { get; set; } = null!;
}
