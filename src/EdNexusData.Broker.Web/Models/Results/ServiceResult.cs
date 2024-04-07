namespace EdNexusData.Broker.Web.Models.Results;

public record ServiceResult<T>(bool IsSuccess, string Message, T? Data) : IServiceResult<T>
{
    public static ServiceResult<T> CreateSuccess(T? data, string message = "Operation succeeded.") =>
        new(true, message, data);

    public static ServiceResult<T> CreateFailure(string message, Exception? exception = null) =>
        new(false, message + (exception != null ? ": " + exception.Message : ""), default);
}
