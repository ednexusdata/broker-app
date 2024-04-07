using Microsoft.AspNetCore.Diagnostics;

namespace EdNexusData.Broker.Web.Exceptions;

public class ForceLogoutExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ForceLogoutExceptionHandler> logger;
    
    public ForceLogoutExceptionHandler(ILogger<ForceLogoutExceptionHandler> logger)
    {
        this.logger = logger;
    }
    
    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ForceLogoutException)
        {
            httpContext.Response.Redirect("/login/logout");
        
            var exceptionMessage = exception.Message;
            logger.LogError(
                "Forced logout; Error Message: {exceptionMessage}, Time of occurrence {time}",
                exceptionMessage, DateTime.UtcNow);
            // Return false to continue with the default behavior
            // - or - return true to signal that this exception is handled
            return ValueTask.FromResult(true);
        }
        
       return ValueTask.FromResult(false);
    }
}