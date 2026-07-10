using DirectoryService.Application.DirectoryServiceManagement.Departments.LinkDepartmentAndLocation;
using DirectoryService.Application.DirectoryServiceManagement.Departments.UnLinkDepartmentAndLocationHandler;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.IntegrationTests.DepartmentFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class UnLinkDepartmentAndLocationTests : DirectoryBaseTests<UnLinkDepartmentAndLocationHandler>
{
    public UnLinkDepartmentAndLocationTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UnLinkDepartmentAndLocation_when_linked_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var (departmentId, locationId) = await CreateLinkedDepartmentAndLocation();

        // act — отвязываем локацию
        var result = await ExecuteHandler(sut =>
        {
            var command = new DepartmentAndLocationCommand(departmentId.Value, locationId.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(locationId.Value, result.Value);
    }

    [Fact]
    public async Task UnLinkDepartmentAndLocation_when_not_linked_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = await CreateDepartmentInDb("Sales", "sales");
        var unrelatedLocationId = await CreateLocationInDb("Barnaul");

        // act — локация существует, но не привязана к этому отделу
        var result = await ExecuteHandler(sut =>
        {
            var command = new DepartmentAndLocationCommand(departmentId.Value, unrelatedLocationId.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
    }

    private async Task<(DepartmentId departmentId, LocationId locationId)> CreateLinkedDepartmentAndLocation()
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

            var departmentId = new DepartmentId(Guid.NewGuid());
            var departmentLocation = DepartmentLocation.Create(departmentId, locationId).Value;
            var department = Department.CreateParent(
                DepartmentName.Create("Procurement").Value,
                Identifier.Create("procurement").Value,
                [departmentLocation],
                departmentId).Value;
            context.Departments.Add(department);

            await context.SaveChangesAsync();
            return (departmentId, locationId);
        });
    }

    private async Task<DepartmentId> CreateDepartmentInDb(string name, string identifier)
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

            var departmentId = new DepartmentId(Guid.NewGuid());
            var departmentLocation = DepartmentLocation.Create(departmentId, locationId).Value;
            var department = Department.CreateParent(
                DepartmentName.Create(name).Value,
                Identifier.Create(identifier).Value,
                [departmentLocation],
                departmentId).Value;
            context.Departments.Add(department);

            await context.SaveChangesAsync();
            return departmentId;
        });
    }

    private async Task<LocationId> CreateLocationInDb(string city)
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create(city).Value,
                Address.Create(city, "Lenina", "1", "000000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(location);
            await context.SaveChangesAsync();
            return locationId;
        });
    }
}
