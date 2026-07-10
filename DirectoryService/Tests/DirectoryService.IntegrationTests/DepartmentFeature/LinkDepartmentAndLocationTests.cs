using DirectoryService.Application.DirectoryServiceManagement.Departments.LinkDepartmentAndLocation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.IntegrationTests.DepartmentFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class LinkDepartmentAndLocationTests : DirectoryBaseTests<LinkDepartmentAndLocationHandler>
{
    public LinkDepartmentAndLocationTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task LinkDepartmentAndLocation_with_valid_ids_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var (departmentId, locationId) = await CreateDepartmentAndExtraLocation();

        // act — привязываем вторую локацию к отделу
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
    public async Task LinkDepartmentAndLocation_with_nonexistent_department_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationInDb();

        // act — отдел не существует
        var result = await ExecuteHandler(sut =>
        {
            var command = new DepartmentAndLocationCommand(Guid.NewGuid(), locationId.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Создаёт отдел с одной локацией и возвращает Id отдела + Id второй (свободной) локации,
    /// готовой для привязки.
    /// </summary>
    private async Task<(DepartmentId departmentId, LocationId extraLocationId)> CreateDepartmentAndExtraLocation()
    {
        return await ExecuteContext(async context =>
        {
            // первая локация — используется при создании отдела
            var primaryLocationId = new LocationId(Guid.NewGuid());
            var primaryLocation = new Location(
                primaryLocationId,
                LocationName.Create("Tomsk").Value,
                Address.Create("Tomsk", "Istochnaya", "42", "634000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(primaryLocation);

            // вторая локация — будет привязана в тесте
            var extraLocationId = new LocationId(Guid.NewGuid());
            var extraLocation = new Location(
                extraLocationId,
                LocationName.Create("Novosibirsk").Value,
                Address.Create("Novosibirsk", "Lenina", "1", "630000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(extraLocation);

            var departmentId = new DepartmentId(Guid.NewGuid());
            var departmentLocation = DepartmentLocation.Create(departmentId, primaryLocationId).Value;
            var department = Department.CreateParent(
                DepartmentName.Create("Logistics").Value,
                Identifier.Create("logistics").Value,
                [departmentLocation],
                departmentId).Value;
            context.Departments.Add(department);

            await context.SaveChangesAsync();
            return (departmentId, extraLocationId);
        });
    }

    private async Task<LocationId> CreateLocationInDb()
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create("Omsk").Value,
                Address.Create("Omsk", "Mira", "5", "644000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(location);
            await context.SaveChangesAsync();
            return locationId;
        });
    }
}
