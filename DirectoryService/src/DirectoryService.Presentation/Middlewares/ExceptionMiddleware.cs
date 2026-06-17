using SharedKernel;

namespace DirectoryService.Presentation.Middlewares;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var correlationId = Guid.NewGuid();
            
            logger.LogError(ex, "Unhandled exception occurred. CorrelationId: {CorrelationId}, RequestPath: {RequestPath}, Method: {Method}", 
                correlationId, context.Request.Path, context.Request.Method);
            
            var responseError = Error.Failure("server.internal",ex.Message);
            var envelope = Envelope.Error(responseError);
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.Headers.Append("X-Correlation-ID", correlationId.ToString());
            
            await context.Response.WriteAsJsonAsync(envelope);
        }
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}