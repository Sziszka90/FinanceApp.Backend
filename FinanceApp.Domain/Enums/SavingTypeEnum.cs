using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Domain.Entities;

public enum SavingTypeEnum
{
  /// <summary>
  /// Liquid, short-term saving
  /// </summary>
  Liquid = 1,

  /// <summary>
  /// Emergency saving
  /// </summary>
  Emergency = 2,

  /// <summary>
  /// Long-term savings
  /// </summary>
  LongTerm = 3
}
