namespace Shared.Core.Http;

public abstract class BaseHttpClient
{
    protected BaseHttpClient(HttpClient httpClient)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    protected HttpClient HttpClient { get; }
}
