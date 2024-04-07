namespace EdNexusData.Broker.Web.Models.Results;

public interface IServiceResult<T>
{
    bool IsSuccess { get; init; }
    string Message { get; init; }
    T? Data { get; init; }
}