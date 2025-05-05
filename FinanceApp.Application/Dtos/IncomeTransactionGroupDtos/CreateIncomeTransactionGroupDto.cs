﻿namespace FinanceApp.Application.Dtos;

public class CreateIncomeTransactionGroupDto
{
  #region Properties

  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? Icon { get; set; }

  #endregion
}