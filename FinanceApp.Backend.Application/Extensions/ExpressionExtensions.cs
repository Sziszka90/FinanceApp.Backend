using System.Linq.Expressions;
using System.Text;

namespace FinanceApp.Backend.Application.Extensions;

public static class ExpressionExtensions
{
  /// <summary>
  /// Gets the property path from an expression.
  /// For example, if the expression is x => x.User.Name, it will return "User.Name".
  /// This is useful for logging or debugging purposes to understand which property is being accessed.
  /// </summary>
  /// <param name="expression"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Gets the MemberExpression from an Expression.
  /// If the expression is a LambdaExpression, it extracts the body.
  /// If the body is a UnaryExpression, it retrieves the operand.
  /// If the expression is not a MemberExpression or LambdaExpression, it returns null.
  /// </summary>
  /// <param name="expression"></param>
  /// <returns></returns>
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
}
