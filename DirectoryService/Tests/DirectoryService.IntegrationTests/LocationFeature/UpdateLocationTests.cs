using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Update;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.LocationFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class UpdateLocationTests : DirectoryBaseTests<UpdateLocationHandler>
{
    public UpdateLocationTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateLocation_with_valid_data_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationInDb("Old Name", "Old City");

        var addressDto = new AddressDto("New City", "Istochnaya", "42", "634000");
        var command = new UpdateLocationCommand(locationId.Value, "New Name", addressDto, "normis");

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(locationId.Value, result.Value);

        await ExecuteContext(async context =>
        {
            var location = await context.Locations.FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);
            Assert.NotNull(location);
            Assert.Equal("New Name", location.Name.Value);
            Assert.Equal("New City", location.Address.City);
            Assert.Equal("normis", location.Timezone.Value);
        });
    }

    [Fact]
    public async Task UpdateLocation_with_nonexistent_id_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var addressDto = new AddressDto("New City", "Istochnaya", "42", "634000");
        var command = new UpdateLocationCommand(Guid.NewGuid(), "New Name", addressDto, "normis");

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsFailure);
    }

    private async Task<LocationId> CreateLocationInDb(string name, string city)
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create(name).Value,
                Address.Create(city, "Istochnaya", Guid.NewGuid().ToString().Substring(0, 4), "634000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(location);
            await context.SaveChangesAsync();
            return locationId;
        });
    }
}
