namespace FoodOrderAPI.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public bool IsNotFound { get; }
    public string Error { get; }

    protected Result(bool isSuccess, string error, bool isNotFound = false)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Resultado de sucesso não pode ter erro.");

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Resultado de falha precisa de mensagem de erro.");

        IsSuccess = isSuccess;
        IsNotFound = isNotFound;
        Error = error ?? string.Empty;
    }

    public static Result Success() => new(true, string.Empty);

    public static Result Failure(string error) => new(false, error);

    public static Result NotFound(string error) => new(false, error, isNotFound: true);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);

    public static Result<T> NotFound<T>(string error) => Result<T>.NotFound(error);
}

public class Result<T> : Result
{
    public T Value { get; }

    private Result(T value, bool isSuccess, string error, bool isNotFound = false)
        : base(isSuccess, error, isNotFound)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, string.Empty);

    public new static Result<T> Failure(string error) => new(default!, false, error);

    public new static Result<T> NotFound(string error) => new(default!, false, error, isNotFound: true);
}
