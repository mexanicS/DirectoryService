using DirectoryService.Application.DirectoryServiceManagement.Locations.Delete;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.LocationFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class DeleteLocationTests : DirectoryBaseTests<DeleteLocationHandler>
{
    public DeleteLocationTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeleteLocation_with_existing_id_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationInDb();

        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new DeleteLocationCommand(locationId.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(locationId.Value, result.Value);

        await ExecuteContext(async context =>
        {
            var exists = await context.Locations.AnyAsync(l => l.Id == locationId, cancellationToken);
            Assert.False(exists);
        });
    }

    [Fact]
    public async Task DeleteLocation_with_nonexistent_id_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var nonexistentId = Guid.NewGuid();

        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new DeleteLocationCommand(nonexistentId);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
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
