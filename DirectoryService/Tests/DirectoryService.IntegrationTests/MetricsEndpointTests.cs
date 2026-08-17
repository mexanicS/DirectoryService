using System.Net;

namespace DirectoryService.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class MetricsEndpointTests(DirectoryTestWebFactory factory) : IClassFixture<DirectoryTestWebFactory>
{
    [Fact]
    public async Task Metrics_endpoint_should_return_prometheus_payload()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("target_info", content);
        Assert.Contains("otel_scope_name", content);
    }
}
