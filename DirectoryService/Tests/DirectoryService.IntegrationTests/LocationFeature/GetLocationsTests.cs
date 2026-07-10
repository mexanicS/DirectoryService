using DirectoryService.Application.DirectoryServiceManagement.Locations.Get;
using DirectoryService.Domain.Locations;

namespace DirectoryService.IntegrationTests.LocationFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class GetLocationsTests : DirectoryBaseTests<GetLocationsHandler>
{
    public GetLocationsTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetLocations_with_valid_query_should_return_paged_list()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        await CreateLocationInDb("Tomsk Office");
        await CreateLocationInDb("Novosibirsk Office");

        var query = new GetLocationsQuery("Office", null, null, null, 1, 20);

        // act
        var result = await ExecuteHandler(sut => sut.Handle(query, cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.TotalCount >= 2);
        Assert.Contains(result.Value.Items, x => x.Name == "Tomsk Office");
        Assert.Contains(result.Value.Items, x => x.Name == "Novosibirsk Office");
    }

    [Fact]
    public async Task GetLocations_with_invalid_page_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var query = new GetLocationsQuery(null, null, null, null, 0, 20); // Page must be > 0

        // act
        var result = await ExecuteHandler(sut => sut.Handle(query, cancellationToken));

        // assert
        Assert.True(result.IsFailure);
    }

    private async Task CreateLocationInDb(string name)
    {
        await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create(name).Value,
                Address.Create("Tomsk", "Istochnaya", Guid.NewGuid().ToString().Substring(0, 4), "634000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(location);
            await context.SaveChangesAsync();
        });
    }
}
