using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities;

public enum TransactionTypeEnum
{
  /// <summary>
  /// Income
  /// </summary>
  Income = 1,

  /// <summary>
  /// Expense
  /// </summary>
  Expense = 2,
}
