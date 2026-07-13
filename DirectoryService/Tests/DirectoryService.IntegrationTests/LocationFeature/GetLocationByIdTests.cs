using DirectoryService.Application.DirectoryServiceManagement.Locations.GetById;
using DirectoryService.Domain.Locations;

namespace DirectoryService.IntegrationTests.LocationFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class GetLocationByIdTests : DirectoryBaseTests<GetLocationByIdHandler>
{
    public GetLocationByIdTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetLocationById_with_existing_id_should_return_location()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationInDb();
        var query = new GetLocationByIdQuery(locationId.Value);

        // act
        var result = await ExecuteHandler(sut => sut.Handle(query, cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(locationId.Value, result.Value.Id);
        Assert.Equal("Tomsk", result.Value.City);
        Assert.Equal("normis", result.Value.Timezone);
    }

    [Fact]
    public async Task GetLocationById_with_nonexistent_id_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var query = new GetLocationByIdQuery(Guid.NewGuid());

        // act
        var result = await ExecuteHandler(sut => sut.Handle(query, cancellationToken));

        // assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetLocationById_with_softdeleted_id_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateSoftDeletedLocationInDb();
        var query = new GetLocationByIdQuery(locationId.Value);

        // act
        var result = await ExecuteHandler(sut => sut.Handle(query, cancellationToken));

        // assert
        Assert.True(result.IsFailure);
    }

    private async Task<LocationId> CreateSoftDeletedLocationInDb()
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create("Tomsk Deleted").Value,
                Address.Create("Tomsk", "Istochnaya", "42", "634000").Value,
                Timezone.Create("normis").Value);
            location.SoftDelete();
            context.Locations.Add(location);
            await context.SaveChangesAsync();
            return locationId;
        });
    }

    private async Task<LocationId> CreateLocationInDb()
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create("Tomsk").Value,
                Address.Create("Tomsk", "Istochnaya", "42", "634000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(location);
            await context.SaveChangesAsync();
            return locationId;
        });
    }
}
