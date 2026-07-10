using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Create;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.LocationFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class CreateLocationTests : DirectoryBaseTests<CreateLocationHandler>
{
    public CreateLocationTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateLocation_with_valid_data_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var addressDto = new AddressDto("Tomsk", "Istochnaya", "42", "634000");
        var command = new CreateLocationDto("Tomsk Office", addressDto, "normis");

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteContext(async context =>
        {
            var location = await context.Locations.FirstOrDefaultAsync(l => l.Id == new LocationId(result.Value), cancellationToken);
            Assert.NotNull(location);
            Assert.Equal("Tomsk Office", location.Name.Value);
            Assert.Equal("Tomsk", location.Address.City);
            Assert.Equal("Istochnaya", location.Address.Street);
            Assert.Equal("42", location.Address.HouseNumber);
            Assert.Equal("634000", location.Address.ZipCode);
            Assert.Equal("normis", location.Timezone.Value);
            Assert.True(location.IsActive);
        });
    }

    [Fact]
    public async Task CreateLocation_with_existing_address_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var addressDto = new AddressDto("Tomsk", "Istochnaya", "42", "634000");
        
        // Save first location with this address
        await ExecuteContext(async context =>
        {
            var existingLocation = new Location(
                new LocationId(Guid.NewGuid()),
                LocationName.Create("Existing Location").Value,
                Address.Create(addressDto.City, addressDto.Street, addressDto.HouseNumber, addressDto.ZipCode).Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(existingLocation);
            await context.SaveChangesAsync(cancellationToken);
        });

        var command = new CreateLocationDto("New Location", addressDto, "normis");

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsFailure);
    }
}
