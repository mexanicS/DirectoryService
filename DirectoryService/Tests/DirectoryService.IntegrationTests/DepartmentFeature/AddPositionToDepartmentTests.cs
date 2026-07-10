using DirectoryService.Application.DirectoryServiceManagement.Departments.AddPositionToDepartment;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.IntegrationTests.DepartmentFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class AddPositionToDepartmentTests : DirectoryBaseTests<AddPositionToDepartmentHandler>
{
    public AddPositionToDepartmentTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddPositionToDepartment_with_valid_ids_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = await CreateDepartmentInDb("HRHR", "hrhr");
        var positionId = await CreatePositionInDb("Developer");

        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new AddPositionToDepartmentCommand(departmentId.Value, positionId.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId.Value, result.Value);
    }

    [Fact]
    public async Task AddPositionToDepartment_with_nonexistent_position_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = await CreateDepartmentInDb("Security", "security");

        // act — позиции не существует в БД
        var result = await ExecuteHandler(sut =>
        {
            var command = new AddPositionToDepartmentCommand(departmentId.Value, Guid.NewGuid());
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
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

    private async Task<PositionId> CreatePositionInDb(string name)
    {
        return await ExecuteContext(async context =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            var position = new Position(
                positionId,
                PositionName.Create(name).Value,
                Description.Create(string.Empty).Value);
            context.Positions.Add(position);
            await context.SaveChangesAsync();
            return positionId;
        });
    }
}
