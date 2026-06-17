using System.Net;
using SharedKernel;

namespace DirectoryService.Presentation.EndpointResults;

public sealed class SuccessResult<TValue>(TValue value) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var envelope = Envelope.Ok(value);

        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}