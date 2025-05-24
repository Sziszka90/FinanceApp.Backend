using System.Linq.Expressions;
using System.Text;

namespace FinanceApp.Application.Extensions;

public static class ExpressionExtensions
{
  #region Methods

  public static string GetPropertyPath(this Expression expression)
  {
    var result = new StringBuilder();
    var memberExpression = GetMemberExpression(expression);
    while (memberExpression != null)
    {
      if (result.Length > 0)
      {
        result.Insert(0, ".");
      }

      result.Insert(0, memberExpression.Member.Name);
      memberExpression = GetMemberExpression(memberExpression.Expression!);
    }

    return result.ToString();
  }

  private static MemberExpression? GetMemberExpression(Expression expression)
  {
    if (expression is MemberExpression memberExpression)
    {
      return memberExpression;
    }

    if (expression is not LambdaExpression lambdaExpression)
    {
      return default;
    }

    return lambdaExpression.Body switch
    {
      MemberExpression lambdaMember => lambdaMember,
      UnaryExpression unaryExpression => (MemberExpression)unaryExpression.Operand,
      _ => default
    };
  }

  #endregion
}
