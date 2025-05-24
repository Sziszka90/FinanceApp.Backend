namespace FinanceApp.Application.Models;

public class Result<T> : Result
{
  public T? Data { get; set; }

  public Result(T data)
  {
    Data = data;
  }

  public Result() { }

  public Result(T data, ApplicationError applicationError) : base(applicationError)
  {
    Data = data;
  }

  public Result(ApplicationError applicationError) : base(applicationError) { }
}

public class Result
{
  public bool IsSuccess => ApplicationError == null;

  public ApplicationError? ApplicationError { get; set; }

  public Result() { }

  public Result(ApplicationError applicationError)
  {
    ApplicationError = applicationError;
  }

  public static Result Success()
  {
    return new Result();
  }

  public static Result<T> Success<T>(T data)
  {
    return new Result<T>(data);
  }

  public static Result Failure(ApplicationError applicationError)
  {
    return new Result(applicationError);
  }

  public static Result<T> Failure<T>(ApplicationError applicationError)
  {
    return new Result<T>(applicationError);
  }

  public static Result<T> Failure<T>(T data, ApplicationError error)
  {
    return new Result<T>(data, error);
  }
}
