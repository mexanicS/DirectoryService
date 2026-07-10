using DirectoryService.Application.DirectoryServiceManagement.Departments.UpdateLocations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.LocationFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class UpdateLocationsByDepartmentTests : DirectoryBaseTests<UpdateLocationsByDepartmentHandler>
{
    public UpdateLocationsByDepartmentTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateLocationsByDepartment_with_valid_locations_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var initialLocationId = await CreateLocationInDb("Location Initial");
        var departmentId = await CreateDepartmentInDb("Dep Name", "depident", initialLocationId);

        var newLocationId1 = await CreateLocationInDb("Location New 1");
        var newLocationId2 = await CreateLocationInDb("Location New 2");

        var command = new UpdateLocationsByDepartmentCommand(departmentId.Value, [newLocationId1.Value, newLocationId2.Value]);

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId.Value, result.Value);

        await ExecuteContext(async context =>
        {
            var links = await context.DepartmentLocations
                .Where(dl => dl.DepartmentId == departmentId)
                .ToListAsync(cancellationToken);

            Assert.Equal(2, links.Count);
            Assert.Contains(links, l => l.LocationId == newLocationId1);
            Assert.Contains(links, l => l.LocationId == newLocationId2);
            Assert.DoesNotContain(links, l => l.LocationId == initialLocationId);
        });
    }

    [Fact]
    public async Task UpdateLocationsByDepartment_with_nonexistent_department_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationInDb("Location Test");
        var nonexistentDepartmentId = Guid.NewGuid();

        var command = new UpdateLocationsByDepartmentCommand(nonexistentDepartmentId, [locationId.Value]);

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsFailure);
    }

    private async Task<LocationId> CreateLocationInDb(string name)
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create(name).Value,
                Address.Create("City", "Street", Guid.NewGuid().ToString().Substring(0, 4), "634000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(location);
            await context.SaveChangesAsync();
            return locationId;
        });
    }

    private async Task<DepartmentId> CreateDepartmentInDb(string name, string identifier, LocationId locationId)
    {
        return await ExecuteContext(async context =>
        {
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
}
