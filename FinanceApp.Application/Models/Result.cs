namespace FinanceApp.Application.Models;

public class Result<T> : Result
{
  #region Properties

  public T? Data { get; set; }

  #endregion

  #region Constructors

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

  #endregion
}

public class Result
{
  #region Properties

  public bool IsSuccess => ApplicationError == null;

  public ApplicationError? ApplicationError { get; set; }

  #endregion

  #region Constructors

  public Result() { }

  public Result(ApplicationError applicationError)
  {
    ApplicationError = applicationError;
  }

  #endregion

  #region Methods

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

  #endregion
}
